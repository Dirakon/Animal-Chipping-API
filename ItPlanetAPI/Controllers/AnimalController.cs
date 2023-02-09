using AutoMapper;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AnimalController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<AnimalController> _logger;

    private readonly IMapper _mapper;

    public AnimalController(ILogger<AnimalController> logger, DatabaseContext context, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
    }


    [HttpGet("Search")]
    [ForbidOnIncorrectAuthorizationHeader]
    public IActionResult Search([FromQuery] AnimalSearchParameters searchParameters)
    {
        if (searchParameters.from < 0 || searchParameters.size <= 0 || searchParameters.chipperId <= 0 ||
            searchParameters.chippingLocationId <= 0 ||
            searchParameters.lifeStatus is not "ALIVE" and not "DEAD" ||
            searchParameters.gender is not "MALE" and not "FEMALE" and not "OTHER") return StatusCode(400);
        var query = _context
            .Animals
            .Where(animal =>
                animal.ChippingDateTime >= searchParameters.startDateTime
                && animal.ChippingDateTime <= searchParameters.endDateTime
            );
        if (searchParameters.chipperId != null)
            query = query.Where(animal => animal.ChipperId == searchParameters.chipperId);

        if (searchParameters.chippingLocationId != null)
            query = query.Where(animal => animal.ChippingLocationId == searchParameters.chippingLocationId);

        if (searchParameters.gender != null) query = query.Where(animal => animal.Gender == searchParameters.gender);

        if (searchParameters.lifeStatus != null)
            query = query.Where(animal => animal.LifeStatus == searchParameters.lifeStatus);

        return Ok(query.OrderBy(animal => animal.Id).Skip(searchParameters.from).Take(searchParameters.size));
    }

    [HttpGet("{id:long}")]
    [ForbidOnIncorrectAuthorizationHeader]
    public IActionResult Get(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var animalSearchedFor = _context.Animals.Find(animal => animal.Id == id);

        return animalSearchedFor.Match<IActionResult>(
            animal => Ok(_mapper.Map<AnimalDto>(animal)),
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

        return Ok(oldAnimalLocation);
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] AnimalRequest animalRequest)
    {
        if (!animalRequest.IsValid()) return BadRequest("Some field is invalid");
        if (animalRequest.HasConflicts()) return Conflict();

        return (await Animal.From(animalRequest, _mapper, _context)).Match<IActionResult>(
            animal =>
            {
                _context.Animals.Add(animal);

                // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
                _context.SaveChangesAsync();

                return Ok(_mapper.Map<AnimalDto>(animal));
            },
            errorCode => StatusCode(errorCode)
        );
    }

    // TODO: delete and animal-specific methods
}

public class AnimalSearchParameters
{
    public DateTime? startDateTime { get; set; } = DateTime.MinValue;
    public DateTime? endDateTime { get; set; } = DateTime.MaxValue;
    public int? chipperId { get; set; }
    public long? chippingLocationId { get; set; }
    public string? lifeStatus { get; set; }
    public string? gender { get; set; }
    public int from { get; set; } = 0;
    public int size { get; set; } = 10;
}