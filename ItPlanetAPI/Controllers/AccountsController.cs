using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly MyDbContext _context;

    public AccountsController(ILogger<AccountsController> logger, MyDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    private static bool IsAccountValid(Account account)
    {
        var allStringFields = new List<string> {account.Email, account.Password, account.FirstName, account.LastName};

        var allFieldsNonEmpty =  allStringFields
            .Select(field => field.Trim())
            .All(field => field != "");

        return allFieldsNonEmpty && IsValidEmail(account.Email);
    }
    private static bool IsValidEmail(string source)
    {
        return new EmailAddressAttribute().IsValid(source);
    }

    private string? GetAuthorization()
    {
        return Request.Headers["Authorization"] == ""? null : (string?)Request.Headers["Authorization"];
    }
    
    [HttpPost(Name = "Registration")]
    public IActionResult Registration([FromBody] Account account)
    {
        if (GetAuthorization() != null)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
        if (!IsAccountValid(account))
        {
            return BadRequest("Some field is invalid");
        }

        var emailAlreadyPresent = _context.Accounts.Any(accountToCheck => accountToCheck.Email == account.Email);
        if (emailAlreadyPresent)
        {
            return Conflict("Account with this e-mail already present");
        }

        if (!_context.Accounts.Any())
            account.Id = 0;
        else
            account.Id = _context.Accounts.Select(acc => acc.Id).Max() + 1;
        
        _context.Accounts.Add(account);
        _context.SaveChangesAsync();
        
        return Ok(new {account.Id,account.FirstName,account.LastName,account.Email});
    }
}