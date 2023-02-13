using System.Diagnostics.Contracts;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using ItPlanetAPI.Models;
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

        return ParseAuthorizationHeader(Request.Headers["Authorization"]) switch
        {
            var (email, password) => await TryToAuthorize(email, password),
            null => AuthenticateResult.Fail("Invalid Authorization header")
        };
    }

    private async Task<AuthenticateResult> TryToAuthorize(string email, string password)
    {
        var user = await _context.Accounts.SingleOrDefaultAsync(u =>
            u.Email == email && u.Password == password);
        if (user == null) return AuthenticateResult.Fail("Invalid email or password");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    [Pure]
    private static (string email, string password)? ParseAuthorizationHeader(string? authorization)
    {
        if (string.IsNullOrEmpty(authorization) ||
            !authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return null;


        var encodedUsernamePassword = authorization["Basic ".Length..].Trim();
        var encoding = Encoding.GetEncoding("iso-8859-1");

        var length = GetBase64BufferLength(encodedUsernamePassword);
        var buffer = new byte[length];

        if (!Convert.TryFromBase64String(encodedUsernamePassword, buffer, out _))
            return null;


        var emailPassword = encoding.GetString(buffer);

        var colonIndex = emailPassword.IndexOf(':');
        if (colonIndex < 0) return null;

        var email = emailPassword[..colonIndex];
        var password = emailPassword[(colonIndex + 1)..];
        return (email, password);
    }

    [Pure]
    private static int GetBase64BufferLength(string encodedString)
    {
        return (encodedString.Length * 3 + 3) / 4 -
               (encodedString.Length > 0 && encodedString[^1] == '='
                   ? encodedString.Length > 1 && encodedString[^2] == '=' ? 2 : 1
                   : 0);
    }
}