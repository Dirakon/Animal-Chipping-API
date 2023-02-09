using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ItPlanetAPI.Models;
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
    private readonly IMapper _mapper;
    private readonly DatabaseContext _context;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(ILogger<AccountsController> logger, DatabaseContext context, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
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
                .Select(account => _mapper.Map<AccountDto>(account))
        );
    }
    [HttpGet("{id:int}")]
    [ForbidOnIncorrectAuthorizationHeader]
    public IActionResult Get(int id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var accountSearchedFor = _context.Accounts.Find(user => user.Id == id);

        return accountSearchedFor.Match<IActionResult>(
            account => Ok(_mapper.Map<AccountDto>(account)),
            NotFound("User with this id is not found")
        );
    }
    
    [HttpPut("{id:int}")]
    [Authorize]
    [ServiceFilter(typeof(AuthorizedUser))]
    public async Task<IActionResult> Put(int id, [FromBody] AccountRequest accountRequest, [OpenApiParameterIgnore] int authorizedUserId)
    {
        if (!accountRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");
        if (id != authorizedUserId) return Forbid();
        
        var oldAccount = await _context.Accounts.SingleOrDefaultAsync(user => user.Id == id);
        if (oldAccount == null)
            return Forbid();
        
        var emailAlreadyPresent = _context.Accounts.Any(accountToCheck => accountToCheck.Email == accountRequest.Email && accountToCheck.Id != id);
        if (emailAlreadyPresent) return Conflict("Account with this e-mail already present");

        _mapper.Map(source: accountRequest, destination: oldAccount);
        
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();
        
        return Ok(_mapper.Map<AccountDto>(oldAccount));

    }

    [HttpPost("Registration")]
    [AllowAnonymousOnly]
    public async Task<IActionResult> Registration([FromBody] AccountRequest accountRequest)
    {
        if (!accountRequest.IsValid()) return BadRequest("Some field is invalid");

        var emailAlreadyPresent = _context.Accounts.Any(accountToCheck => accountToCheck.Email == accountRequest.Email);
        if (emailAlreadyPresent) return Conflict("Account with this e-mail already present");

        var account = _mapper.Map<Account>(accountRequest);
        if (!_context.Accounts.Any())
            account.Id = 1;
        else
            account.Id = await _context.Accounts.Select(acc => acc.Id).MaxAsync() + 1;

        _context.Accounts.Add(account);
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();

        return Ok(_mapper.Map<AccountDto>(account));
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