using AutoMapper;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<AccountsController> _logger;
    private readonly IMapper _mapper;

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
        if (searchParameters.From < 0 || searchParameters.Size <= 0) return StatusCode(400);
        return Ok(
            _context.Accounts
                .Where(account =>
                    account.Email.Contains(searchParameters.EmailName) &&
                    account.FirstName.Contains(searchParameters.FirstName) &&
                    account.LastName.Contains(searchParameters.LastName)
                )
                .OrderBy(account => account.Id)
                .Skip(searchParameters.From)
                .Take(searchParameters.Size)
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
    public async Task<IActionResult> Put(int id, [FromBody] AccountRequest accountRequest,
        [OpenApiParameterIgnore] int authorizedUserId)
    {
        if (!accountRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");
        if (id != authorizedUserId) return Forbid();

        var oldAccount = await _context.Accounts.SingleOrDefaultAsync(user => user.Id == id);
        if (oldAccount == null)
            return Forbid();

        var emailAlreadyPresent = _context.Accounts.Any(accountToCheck =>
            accountToCheck.Email == accountRequest.Email && accountToCheck.Id != id);
        if (emailAlreadyPresent) return Conflict("Account with this e-mail already present");

        _mapper.Map(accountRequest, oldAccount);

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

    [HttpDelete("{id:int}")]
    [Authorize]
    [ServiceFilter(typeof(AuthorizedUser))]
    public async Task<IActionResult> Delete(int id,
        [OpenApiParameterIgnore] int authorizedUserId)
    {
        if (id <= 0) return BadRequest("Id must be positive");
        if (id != authorizedUserId) return Forbid();

        var accountToRemove =
            await _context.Accounts.SingleOrDefaultAsync(account => account.Id == id);
        if (accountToRemove == null)
            return Forbid();

        var connectedToAnimals = accountToRemove.ChippedAnimals.Any();
        if (connectedToAnimals) return BadRequest("Animal connected with this account is present");

        _context.Accounts.Remove(accountToRemove);

        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();

        return Ok();
    }
}

public class UserSearchParameters
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string EmailName { get; set; } = "";
    public int From { get; set; } = 0;
    public int Size { get; set; } = 10;
}