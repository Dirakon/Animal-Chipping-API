using System.Buffers;
using System.Diagnostics.Contracts;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ItPlanetAPI;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly DatabaseContext _context;

    public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock, DatabaseContext context)
        : base(options, logger, encoder, clock)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization header");

        return await ParseAuthorizationHeader(Request.Headers["Authorization"]).Match(
            async credentials =>
            {
                var user = await _context.Accounts.SingleOrDefaultAsync(u =>
                    u.Email == credentials.email && u.Password == credentials.password);
                if (user == null) return AuthenticateResult.Fail("Invalid email or password");

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            },
            Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header"))
        );
    }

    [Pure]
    private static Option<(string email, string password)> ParseAuthorizationHeader(string? authorization)
    {
        if (string.IsNullOrEmpty(authorization) ||
            !authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return Option<(string email, string password)>.None;


        var encodedUsernamePassword = authorization["Basic ".Length..].Trim();
        var encoding = Encoding.GetEncoding("iso-8859-1");
        
        var length = GetBase64BufferLength(encodedUsernamePassword);
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        
        if (!Convert.TryFromBase64String(encodedUsernamePassword, buffer, out _))
            return Option<(string email, string password)>.None;

        var usernamePassword = encoding.GetString(buffer);
        ArrayPool<byte>.Shared.Return(buffer);
        
        var colonIndex = usernamePassword.IndexOf(':');
        if (colonIndex < 0) return Option<(string email, string password)>.None;

        var email = usernamePassword[..colonIndex];
        var password = usernamePassword[(colonIndex + 1)..];
        return (email, password);
    }

    [Pure]
    private static int GetBase64BufferLength(string encodedString)
    {
        return (encodedString.Length * 3 + 3) / 4 -
               (encodedString.Length > 0 && encodedString[^1] == '=' ?
                   encodedString.Length > 1 && encodedString[^2] == '=' ?
                       2 : 1 : 0);
    }
}