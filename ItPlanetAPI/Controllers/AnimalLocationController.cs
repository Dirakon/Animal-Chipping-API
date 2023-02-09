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
    public IActionResult Get(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var animalLocationSearchedFor = _context.AnimalLocations.Find(animalLocation => animalLocation.Id == id);

        return animalLocationSearchedFor.Match<IActionResult>(
            animalLocation => Ok(_mapper.Map<AnimalLocationDto>(animalLocation)),
            NotFound("Location with this id is not found")
        );
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Put(long id, [FromBody] AnimalLocationRequest animalLocationRequest)
    {
        if (!animalLocationRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimalLocation =
            await _context.AnimalLocations.SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldAnimalLocation == null)
            return NotFound();

        var coordinatesAlreadyPresent = _context.AnimalLocations.Any(locationToCheck =>
            locationToCheck.Latitude.AlmostEqualTo(animalLocationRequest.Latitude) &&
            locationToCheck.Longitude.AlmostEqualTo(animalLocationRequest.Longitude)
        );
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        _mapper.Map(animalLocationRequest, oldAnimalLocation);

        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();

        return Ok(_mapper.Map<AnimalLocationDto>(oldAnimalLocation));
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] AnimalLocationRequest animalLocationRequest)
    {
        if (!animalLocationRequest.IsValid()) return BadRequest("Some field is invalid");

        var coordinatesAlreadyPresent = _context.AnimalLocations.Any(locationToCheck =>
            locationToCheck.Latitude.AlmostEqualTo(animalLocationRequest.Latitude) &&
            locationToCheck.Longitude.AlmostEqualTo(animalLocationRequest.Longitude)
        );
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        var animalLocation = _mapper.Map<AnimalLocation>(animalLocationRequest);
        if (!_context.AnimalLocations.Any())
            animalLocation.Id = 1;
        else
            animalLocation.Id =
                await _context.AnimalLocations.Select(locationToCheck => locationToCheck.Id).MaxAsync() + 1;

        _context.AnimalLocations.Add(animalLocation);
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();

        return Ok(_mapper.Map<AnimalLocationDto>(animalLocation));
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimalLocation =
            await _context.AnimalLocations.SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldAnimalLocation == null)
            return NotFound();

        var locationIsPresentInAnimals =
            oldAnimalLocation.AnimalsChippedHere.Any() || oldAnimalLocation.AnimalsVisitedHere.Any();
        if (locationIsPresentInAnimals) return BadRequest("Animal connected with this location is present");

        _context.AnimalLocations.Remove(oldAnimalLocation);

        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();

        return Ok();
    }
}