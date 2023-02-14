using AutoMapper;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<LocationsController> _logger;

    private readonly IMapper _mapper;

    public LocationsController(ILogger<LocationsController> logger, DatabaseContext context, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("{id:long}")]
    [ForbidOnIncorrectAuthorizationHeader]
    public async Task<IActionResult> Get(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var locationSearchedFor =
            await _context.Locations.FirstOrDefaultAsync(animalLocation => animalLocation.Id == id);

        return locationSearchedFor switch
        {
            { } location => Ok(_mapper.Map<LocationDto>(location)),
            null => NotFound("Location with this id is not found")
        };
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Put(long id, [FromBody] LocationRequest locationRequest)
    {
        if (!locationRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");

        var oldLocation =
            await _context.Locations.SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldLocation == null)
            return NotFound();

        var coordinatesAlreadyPresent = await _context.Locations.AnyAsync(Location.IsAlmostTheSameAs(locationRequest));
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        _mapper.Map(locationRequest, oldLocation);

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<LocationDto>(oldLocation));
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] LocationRequest locationRequest)
    {
        if (!locationRequest.IsValid()) return BadRequest("Some field is invalid");

        var coordinatesAlreadyPresent = await _context.Locations.AnyAsync(Location.IsAlmostTheSameAs(locationRequest));
            
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        var location = _mapper.Map<Location>(locationRequest);

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return new ObjectResult(_mapper.Map<LocationDto>(location)) { StatusCode = StatusCodes.Status201Created };
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var oldLocation =
            await _context.Locations
                .Include(location=>location.AnimalsChippedHere)
                .Include(location=>location.AnimalsVisitedHere)
                .SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldLocation == null)
            return NotFound();

        var locationIsPresentInAnimals =
            oldLocation.AnimalsChippedHere.Any() || oldLocation.AnimalsVisitedHere.Any();
        if (locationIsPresentInAnimals) return BadRequest("Animal connected with this location is present");

        _context.Locations.Remove(oldLocation);

        await _context.SaveChangesAsync();

        return Ok();
    }
}