using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ItPlanetAPI;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly DatabaseContext _context;
    
    public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, DatabaseContext context)
        : base(options, logger, encoder, clock)
    {
        _context = context;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    { 
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization header");
        }

        Account? user = 
            await ParseAuthorizationHeader(Request.Headers["Authorization"]).Match(
                Some: credentials => _context.Accounts.SingleOrDefaultAsync(u => u.Email == credentials.email && u.Password == credentials.password),
                None: Task.FromResult((Account?)null)
            );

        // if (user == null)
        // {
        //     return AuthenticateResult.Fail("Invalid Authorization header"); 
        // }
        

        if (user == null)
        {
            return AuthenticateResult.Fail("Invalid Authorization header");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    [AllowAnonymous]
    [Authorize]
    private Option<(string email, string password)> ParseAuthorizationHeader(string? authorization)
    {
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return Option<(string email, string password)>.None;


        string encodedUsernamePassword = authorization["Basic ".Length..].Trim();
        Encoding encoding = Encoding.GetEncoding("iso-8859-1");
        string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

        int colonIndex = usernamePassword.IndexOf(':');
        if (colonIndex < 0)
        {
            return Option<(string email, string password)>.None;
        }

        string email = usernamePassword[..colonIndex];
        string password = usernamePassword[(colonIndex + 1)..];
        return Option<(string email, string  password)>.Some((email,password));
    }
}