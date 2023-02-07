using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly DatabaseContext _context;

    public AccountsController(ILogger<AccountsController> logger, DatabaseContext context)
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


    [HttpGet("Search")]
    [Authorize]
    public IActionResult Search([FromQuery] UserSearchParameters searchParameters)
    {
        if (searchParameters.from < 0 || searchParameters.size <= 0)
        {
            return StatusCode(400);
        }
        return Ok(
            _context.Accounts
                .Where(account=>
                     account.Email.Contains(searchParameters.emailName) &&
                     account.FirstName.Contains(searchParameters.firstName) &&
                     account.LastName.Contains(searchParameters.lastName)
                )
                .OrderBy(account => account.Id)
                .Skip(searchParameters.from)
                .Take(searchParameters.size)
                .Select(account=>account.GetWithoutPassword())
            );
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public IActionResult Get(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be positive");
        }
        
        var accountSearchedFor = _context.Accounts.Find(user => user.Id == id);

        return accountSearchedFor.Match<IActionResult>(
            Some: account => Ok(account.GetWithoutPassword()),
            None: NotFound("User with this id is not found")
            );

    }

    [HttpPost("Registration")]
    [AllowAnonymousOnly]
    public IActionResult Registration([FromBody] Account account)
    {
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
            account.Id = 1;
        else
            account.Id = _context.Accounts.Select(acc => acc.Id).Max() + 1;
        
        _context.Accounts.Add(account);
        _context.SaveChangesAsync();
        
        return Ok(account.GetWithoutPassword());
    }
}

public class UserSearchParameters
{
    public string firstName { get; set; } = "";
    public string lastName { get; set; } = "";
    public string emailName { get; set; } = "";
    public int from { get; set; } = 0;
    public int size { get; set; } = 10;
}