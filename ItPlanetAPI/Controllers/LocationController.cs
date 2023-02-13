using AutoMapper;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<LocationController> _logger;

    private readonly IMapper _mapper;

    public LocationController(ILogger<LocationController> logger, DatabaseContext context, IMapper mapper)
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

        var coordinatesAlreadyPresent = _context.Locations.Any(locationToCheck =>
            locationToCheck.Latitude.AlmostEqualTo(locationRequest.Latitude) &&
            locationToCheck.Longitude.AlmostEqualTo(locationRequest.Longitude)
        );
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

        var coordinatesAlreadyPresent = _context.Locations.Any(locationToCheck =>
            locationToCheck.Latitude.AlmostEqualTo(locationRequest.Latitude) &&
            locationToCheck.Longitude.AlmostEqualTo(locationRequest.Longitude)
        );
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        var location = _mapper.Map<Location>(locationRequest);
        if (!_context.Locations.Any())
            location.Id = 1;
        else
            location.Id =
                await _context.Locations.Select(locationToCheck => locationToCheck.Id).MaxAsync() + 1;

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<LocationDto>(location));
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