using System.ComponentModel.DataAnnotations;
using LanguageExt.SomeHelp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(ILogger<AccountsController> logger, DatabaseContext context)
    {
        _logger = logger;
        _context = context;
    }

    private static bool IsAccountValid(Account account)
    {
        var allStringFields = new List<string> {account.Email, account.Password, account.FirstName, account.LastName};

        var allFieldsNonEmpty = allStringFields
            .Select(field => field.Trim())
            .All(field => field != "");

        return allFieldsNonEmpty && IsValidEmail(account.Email);
    }

    private static bool IsValidEmail(string source)
    {
        return new EmailAddressAttribute().IsValid(source);
    }


    [HttpGet("Search")]
    [ForbidOnIncorrectAuthorizationHeader]
    public IActionResult Search([FromQuery] UserSearchParameters searchParameters)
    {
        if (searchParameters.from < 0 || searchParameters.size <= 0) return StatusCode(400);
        return Ok(
            _context.Accounts
                .Where(account =>
                    account.Email.Contains(searchParameters.emailName) &&
                    account.FirstName.Contains(searchParameters.firstName) &&
                    account.LastName.Contains(searchParameters.lastName)
                )
                .OrderBy(account => account.Id)
                .Skip(searchParameters.from)
                .Take(searchParameters.size)
                .Select(account => account.GetWithoutPassword())
        );
    }
    [HttpGet("{id:int}")]
    [ForbidOnIncorrectAuthorizationHeader]
    public IActionResult Get(int id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var accountSearchedFor = _context.Accounts.Find(user => user.Id == id);

        return accountSearchedFor.Match<IActionResult>(
            account => Ok(account.GetWithoutPassword()),
            NotFound("User with this id is not found")
        );
    }
    
    [HttpPut("{id:int}")]
    [Authorize]
    [ServiceFilter(typeof(AuthorizedUser))]
    public async Task<IActionResult> Put(int id, [FromBody] Account account,[FromServices] int authorizedUserId)
    {
        if (!IsAccountValid(account)) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");
        if (id != authorizedUserId) return Forbid();
        
        var oldAccount = await _context.Accounts.SingleOrDefaultAsync(user => user.Id == id);
        if (oldAccount == null)
            return Forbid();
        
        var emailAlreadyPresent = _context.Accounts.Any(accountToCheck => accountToCheck.Email == account.Email && accountToCheck.Id != id);
        if (emailAlreadyPresent) return Conflict("Account with this e-mail already present");

        oldAccount.Email = account.Email;
        oldAccount.FirstName = account.FirstName;
        oldAccount.LastName = account.LastName;
        oldAccount.Password = account.Password;
        
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();
        
        return Ok(oldAccount.GetWithoutPassword());

    }

    [HttpPost("Registration")]
    [AllowAnonymousOnly]
    public async Task<IActionResult> Registration([FromBody] Account account)
    {
        if (!IsAccountValid(account)) return BadRequest("Some field is invalid");

        var emailAlreadyPresent = _context.Accounts.Any(accountToCheck => accountToCheck.Email == account.Email);
        if (emailAlreadyPresent) return Conflict("Account with this e-mail already present");

        if (!_context.Accounts.Any())
            account.Id = 1;
        else
            account.Id = await _context.Accounts.Select(acc => acc.Id).MaxAsync() + 1;

        _context.Accounts.Add(account);
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
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