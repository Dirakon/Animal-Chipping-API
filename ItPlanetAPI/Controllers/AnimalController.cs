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
        if (searchParameters.From < 0 || searchParameters.Size <= 0 || searchParameters.ChipperId is <= 0 ||
            searchParameters.ChippingLocationId is <= 0 ||
            searchParameters.LifeStatus is not "ALIVE" and not "DEAD" and not null ||
            searchParameters.Gender is not "MALE" and not "FEMALE" and not "OTHER" and not null) return StatusCode(400);
        var query = _context
            .Animals
            .Where(animal =>
                animal.ChippingDateTime >= searchParameters.StartDateTime
                && animal.ChippingDateTime <= searchParameters.EndDateTime
            );
        if (searchParameters.ChipperId != null)
            query = query.Where(animal => animal.ChipperId == searchParameters.ChipperId);

        if (searchParameters.ChippingLocationId != null)
            query = query.Where(animal => animal.ChippingLocationId == searchParameters.ChippingLocationId);

        if (searchParameters.Gender != null) query = query.Where(animal => animal.Gender == searchParameters.Gender);

        if (searchParameters.LifeStatus != null)
            query = query.Where(animal => animal.LifeStatus == searchParameters.LifeStatus);

        return Ok(query.OrderBy(animal => animal.Id).Skip(searchParameters.From).Take(searchParameters.Size));
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
    public async Task<IActionResult> Put(long id, [FromBody] AnimalUpdateRequest animalRequest)
    {
        if (!animalRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimal =
            await _context.Animals.SingleOrDefaultAsync(animal => animal.Id == id);
        if (oldAnimal == null)
            return NotFound();

        return (await oldAnimal.TryTakeValuesOf(animalRequest, _mapper, _context)).Match<IActionResult>(
            _ =>
            {
                // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
                _context.SaveChangesAsync();

                return Ok(_mapper.Map<AnimalDto>(oldAnimal));
            },
            errorCode => StatusCode(errorCode)
        );
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] AnimalCreationRequest animalRequest)
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
    public DateTime? StartDateTime { get; set; } = DateTime.MinValue;
    public DateTime? EndDateTime { get; set; } = DateTime.MaxValue;
    public int? ChipperId { get; set; }
    public long? ChippingLocationId { get; set; }
    public string? LifeStatus { get; set; }
    public string? Gender { get; set; }
    public int From { get; set; } = 0;
    public int Size { get; set; } = 10;
}