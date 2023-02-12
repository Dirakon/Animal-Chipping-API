using AutoMapper;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AnimalLocationController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<AnimalLocationController> _logger;

    private readonly IMapper _mapper;

    public AnimalLocationController(ILogger<AnimalLocationController> logger, DatabaseContext context, IMapper mapper)
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

        var animalLocationSearchedFor = await _context.Locations.FirstOrDefaultAsync(animalLocation => animalLocation.Id == id);

        return animalLocationSearchedFor switch{
            {} animalLocation => Ok(_mapper.Map<LocationDto>(animalLocation)),
            null => NotFound("Location with this id is not found")
        };
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Put(long id, [FromBody] LocationRequest locationRequest)
    {
        if (!locationRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimalLocation =
            await _context.Locations.SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldAnimalLocation == null)
            return NotFound();

        var coordinatesAlreadyPresent = _context.Locations.Any(locationToCheck =>
            locationToCheck.Latitude.AlmostEqualTo(locationRequest.Latitude) &&
            locationToCheck.Longitude.AlmostEqualTo(locationRequest.Longitude)
        );
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        _mapper.Map(locationRequest, oldAnimalLocation);

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<LocationDto>(oldAnimalLocation));
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

        var animalLocation = _mapper.Map<Location>(locationRequest);
        if (!_context.Locations.Any())
            animalLocation.Id = 1;
        else
            animalLocation.Id =
                await _context.Locations.Select(locationToCheck => locationToCheck.Id).MaxAsync() + 1;

        _context.Locations.Add(animalLocation);
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<LocationDto>(animalLocation));
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimalLocation =
            await _context.Locations.SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldAnimalLocation == null)
            return NotFound();

        var locationIsPresentInAnimals =
            oldAnimalLocation.AnimalsChippedHere.Any() || oldAnimalLocation.AnimalsVisitedHere.Any();
        if (locationIsPresentInAnimals) return BadRequest("Animal connected with this location is present");

        _context.Locations.Remove(oldAnimalLocation);

        await _context.SaveChangesAsync();

        return Ok();
    }
}