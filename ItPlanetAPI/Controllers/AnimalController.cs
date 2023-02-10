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
    public IActionResult SearchAnimals([FromQuery] AnimalSearchRequest searchRequest)
    {
        if (searchRequest.From < 0 || searchRequest.Size <= 0 || searchRequest.ChipperId is <= 0 ||
            searchRequest.ChippingLocationId is <= 0 ||
            searchRequest.LifeStatus is not "ALIVE" and not "DEAD" and not null ||
            searchRequest.Gender is not "MALE" and not "FEMALE" and not "OTHER" and not null) return StatusCode(400);
        var query = _context
            .Animals
            .Where(animal =>
                animal.ChippingDateTime >= searchRequest.StartDateTime
                && animal.ChippingDateTime <= searchRequest.EndDateTime
            );
        if (searchRequest.ChipperId != null)
            query = query.Where(animal => animal.ChipperId == searchRequest.ChipperId);

        if (searchRequest.ChippingLocationId != null)
            query = query.Where(animal => animal.ChippingLocationId == searchRequest.ChippingLocationId);

        if (searchRequest.Gender != null) query = query.Where(animal => animal.Gender == searchRequest.Gender);

        if (searchRequest.LifeStatus != null)
            query = query.Where(animal => animal.LifeStatus == searchRequest.LifeStatus);

        return Ok(query.OrderBy(animal => animal.Id).Skip(searchRequest.From).Take(searchRequest.Size));
    }

    [HttpGet("{id:long}")]
    [ForbidOnIncorrectAuthorizationHeader]
    public IActionResult GetAnimal(long id)
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
    public async Task<IActionResult> UpdateAnimal(long id, [FromBody] AnimalUpdateRequest animalRequest)
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
    public async Task<IActionResult> CreateAnimal([FromBody] AnimalCreationRequest animalRequest)
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
    
    

    [HttpPost("{animalId:long}/types/{typeId:long}")]
    [Authorize]
    public async Task<IActionResult> AddType(long typeId, long animalId)
    {
        if (typeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = _context.Animals.SingleOrDefault(animal => animal.Id == animalId);
        if (animal == null)
        {
            return NotFound("Animal not found");
        }

        if (animal.AnimalTypes.Any(animalType => animalType.TypeId == typeId))
        {
            return Conflict("Animal type already present");
        }

        var newType = _context.AnimalTypes.SingleOrDefault(type => type.Id == typeId);
        if (newType == null)
        {
            return NotFound("Type not found");
        }

        var newRelationship = new AnimalAndTypeRelationship()
            {Animal = animal, AnimalId = animalId, Type = newType, TypeId = typeId};
        animal.AnimalTypes.Add(newRelationship);
        newType.Animals.Add(newRelationship);

        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();


        return Ok(_mapper.Map<AnimalDto>(animal));

    }
    
    [HttpPut("{animalId:long}/types")]
    [Authorize]
    public async Task<IActionResult> UpdateType(long animalId, [FromBody] AnimalTypeUpdateRequest updateRequest)
    {
        if (updateRequest.OldTypeId <= 0 || updateRequest.NewTypeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = _context.Animals.SingleOrDefault(animal => animal.Id == animalId);
        if (animal == null)
        {
            return NotFound("Animal not found");
        }

        var oldTypeRelationship = animal.AnimalTypes.SingleOrDefault(type => type.TypeId == updateRequest.OldTypeId);
        if (oldTypeRelationship == null)
        {
            return NotFound("Old type is not present on animal");
        }

        if (animal.AnimalTypes.Any(type => type.TypeId == updateRequest.NewTypeId))
        {
            return Conflict("Animal already has the new type");
        }

        var newType = _context.AnimalTypes.SingleOrDefault(animalType => animalType.Id == updateRequest.NewTypeId);
        if (newType == null)
        {
            return NotFound("New type cannot be found in the database");
        }
        
        var oldType = oldTypeRelationship.Type;
        oldType.Animals.Remove(oldTypeRelationship);
        
        oldTypeRelationship.TypeId = updateRequest.NewTypeId;
        oldTypeRelationship.Type = newType;


        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();


        return Ok(_mapper.Map<AnimalDto>(animal));

    }
    
    [HttpDelete("{animalId:long}/types/{typeId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteType(long animalId,  long typeId)
    {
        if (typeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = _context.Animals.SingleOrDefault(animal => animal.Id == animalId);
        if (animal == null)
        {
            return NotFound("Animal not found");
        }

        var typeRelationship = animal.AnimalTypes.SingleOrDefault(type => type.TypeId == typeId);
        if (typeRelationship == null)
        {
            return NotFound("The type is not present on animal");
        }

        if (animal.AnimalTypes.Count == 1)
        {
            return BadRequest("The type is the only type the animal has.");
        }
        
        var oldType = typeRelationship.Type;
        oldType.Animals.Remove(typeRelationship);
        animal.AnimalTypes.Remove(typeRelationship);


        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();


        return Ok(_mapper.Map<AnimalDto>(animal));
        
    }
    
    [HttpPost("{animalId:long}/locations/{pointId:long}")]
    [Authorize]
    public async Task<IActionResult> AddLocation(long animalId, long pointId)
    {
        if (pointId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = _context.Animals.SingleOrDefault(animal => animal.Id == animalId);
        if (animal == null)
        {
            return NotFound("Animal not found");
        }

        var typeRelationship = animal.AnimalTypes.SingleOrDefault(type => type.TypeId == typeId);
        if (typeRelationship == null)
        {
            return NotFound("The type is not present on animal");
        }

        if (animal.AnimalTypes.Count == 1)
        {
            return BadRequest("The type is the only type the animal has.");
        }
        
        var oldType = typeRelationship.Type;
        oldType.Animals.Remove(typeRelationship);
        animal.AnimalTypes.Remove(typeRelationship);


        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();


        return Ok(_mapper.Map<AnimalDto>(animal));

    }
}