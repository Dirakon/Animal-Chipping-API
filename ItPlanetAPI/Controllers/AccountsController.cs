using AutoMapper;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Middleware;
using ItPlanetAPI.Models;
using ItPlanetAPI.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[Route("[controller]")]
public class AccountsController : BaseEntityController
{
    public AccountsController(ILogger<AccountsController> logger, DatabaseContext context, IMapper mapper) : base(
        logger, context, mapper)
    {
    }

    [HttpGet("Search")]
    public IActionResult Search([FromQuery] AccountSearchRequest searchRequest)
    {
        return Ok(
            _context.Accounts
                .Where(account =>
                    account.Email.Contains(searchRequest.Email) &&
                    account.FirstName.Contains(searchRequest.FirstName) &&
                    account.LastName.Contains(searchRequest.LastName)
                )
                .OrderBy(account => account.Id)
                .Skip(searchRequest.From)
                .Take(searchRequest.Size)
                .Select(account => _mapper.Map<AccountDto>(account))
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var accountSearchedFor = await _context.Accounts.FirstOrDefaultAsync(user => user.Id == id);

        return accountSearchedFor switch
        {
            { } account => Ok(_mapper.Map<AccountDto>(account)),
            null => NotFound("User with this id is not found")
        };
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ServiceFilter(typeof(AuthorizedUser))]
    public async Task<IActionResult> Update(int id, [FromBody] AccountCreationRequest accountCreationRequest,
        [OpenApiParameterIgnore] int authorizedUserId)
    {
        if (id <= 0) return BadRequest("Id must be positive");
        if (id != authorizedUserId) return Forbid();

        var oldAccount = await _context.Accounts.SingleOrDefaultAsync(user => user.Id == id);
        if (oldAccount == null)
            return Forbid();

        var emailAlreadyPresent = _context.Accounts.Any(accountToCheck =>
            accountToCheck.Email == accountCreationRequest.Email && accountToCheck.Id != id);
        if (emailAlreadyPresent) return Conflict("Account with this e-mail already present");

        _mapper.Map(accountCreationRequest, oldAccount);

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AccountDto>(oldAccount));
    }

    [HttpPost]
    [Route("/Registration")]
    [AllowAnonymousOnly]
    public async Task<IActionResult> Create([FromBody] AccountCreationRequest accountCreationRequest)
    {
        var emailAlreadyPresent =
            _context.Accounts.Any(accountToCheck => accountToCheck.Email == accountCreationRequest.Email);
        if (emailAlreadyPresent) return Conflict("Account with this e-mail already present");

        var account = _mapper.Map<Account>(accountCreationRequest);
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return new ObjectResult(_mapper.Map<AccountDto>(account)) {StatusCode = StatusCodes.Status201Created};
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
            await _context.Accounts.Include(account => account.ChippedAnimals)
                .SingleOrDefaultAsync(account => account.Id == id);
        if (accountToRemove == null)
            return Forbid();

        var connectedToAnimals = accountToRemove.ChippedAnimals.Any();
        if (connectedToAnimals) return BadRequest("Animal connected with this account is present");

        _context.Accounts.Remove(accountToRemove);

        await _context.SaveChangesAsync();
        return Ok();
    }
}