using AutoMapper;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Models;
using ItPlanetAPI.Requests;
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
        if (!searchRequest.IsValid()) return StatusCode(400);
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
        
        
        return Ok(
            query
                .OrderBy(animal => animal.Id)
                .Skip(searchRequest.From)
                .Take(searchRequest.Size)
                .Include(animal=>animal.AnimalTypes)
                .Include(animal=>animal.VisitedLocations)
                .Select(animal=>_mapper.Map<AnimalDto>(animal))
            );
    }


    [HttpGet("{id:long}")]
    [ForbidOnIncorrectAuthorizationHeader]
    public async Task<IActionResult> GetAnimal(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var animalSearchedFor = await _context.Animals
            .Include(animal=>animal.AnimalTypes)
            .Include(animal=>animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == id);

        return animalSearchedFor switch
        {
            null => NotFound("Location with this id is not found"),
            _ => Ok(_mapper.Map<AnimalDto>(animalSearchedFor))
        };
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> UpdateAnimal(long id, [FromBody] AnimalUpdateRequest animalRequest)
    {
        if (!animalRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimal =
            await _context.Animals
                .Include(animal=>animal.AnimalTypes)
                .Include(animal=>animal.VisitedLocations)
                .FirstOrDefaultAsync(animal => animal.Id == id);
        if (oldAnimal == null)
            return NotFound("Animal cannot be found");
        if (!oldAnimal.IsRequestAppropriate(animalRequest))
            return BadRequest();

        if (!await oldAnimal.TryTakeValuesOf(animalRequest, _mapper, _context))
            return NotFound("Some entities in request cannot be found");

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AnimalDto>(oldAnimal));
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> CreateAnimal([FromBody] AnimalCreationRequest animalRequest)
    {
        if (!animalRequest.IsValid()) return BadRequest("Some field is invalid");
        if (animalRequest.HasConflicts()) return Conflict();

        if (await Animal.TryCreateFrom(animalRequest, _mapper, _context) is { } animal)
        {
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<AnimalDto>(animal));
        }

        return NotFound("Some entities in request cannot be found");
    }

    // TODO: delete and animal-specific methods


    [HttpPost("{animalId:long}/types/{typeId:long}")]
    [Authorize]
    public async Task<IActionResult> AddType(long typeId, long animalId)
    {
        if (typeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = await _context.Animals
            .Include(animal=>animal.AnimalTypes)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        if (animal.AnimalTypes.Any(animalType => animalType.TypeId == typeId))
            return Conflict("Animal type already present");

        var newType = _context.AnimalTypes.SingleOrDefault(type => type.Id == typeId);
        if (newType == null) return NotFound("Type not found");

        var newRelationship = new AnimalAndTypeRelationship
            {Animal = animal, AnimalId = animalId, Type = newType, TypeId = typeId};

        newRelationship.InitializeRelationship();

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<AnimalDto>(animal));
    }

    [HttpPut("{animalId:long}/types")]
    [Authorize]
    public async Task<IActionResult> UpdateType(long animalId, [FromBody] AnimalTypeUpdateRequest updateRequest)
    {
        if (updateRequest.OldTypeId <= 0 || updateRequest.NewTypeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = await _context.Animals
            .Include(animal=>animal.AnimalTypes)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var oldTypeRelationship = animal.AnimalTypes.SingleOrDefault(type => type.TypeId == updateRequest.OldTypeId);
        if (oldTypeRelationship == null) return NotFound("Old type is not present on animal");

        if (animal.AnimalTypes.Any(type => type.TypeId == updateRequest.NewTypeId))
            return Conflict("Animal already has the new type");

        var newType = _context.AnimalTypes.SingleOrDefault(animalType => animalType.Id == updateRequest.NewTypeId);
        if (newType == null) return NotFound("New type cannot be found in the database");

        oldTypeRelationship.ChangeTypeTo(newType);
        
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<AnimalDto>(animal));
    }

    [HttpDelete("{animalId:long}/types/{typeId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteType(long animalId, long typeId)
    {
        if (typeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = await _context.Animals
            .Include(animal=>animal.AnimalTypes)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var typeRelationship = animal.AnimalTypes.SingleOrDefault(type => type.TypeId == typeId);
        if (typeRelationship == null) return NotFound("The type is not present on animal");

        if (animal.AnimalTypes.Count == 1) return BadRequest("The type is the only type the animal has.");

        typeRelationship.Remove(_context);

        await _context.SaveChangesAsync();


        return Ok(_mapper.Map<AnimalDto>(animal));
    }

    [HttpPost("{animalId:long}/locations/{pointId:long}")]
    [Authorize]
    public async Task<IActionResult> AddLocation(long animalId, long pointId)
    {
        if (pointId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = await _context.Animals
            .Include(animal=>animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        if (animal.LifeStatus == "DEAD") return BadRequest("Cannot move dead animal.");

        if (pointId == animal.ChippingLocationId && !animal.VisitedLocations.Any())
            return BadRequest("Animal cannot move from chipping location to chipping location");

        if (animal.VisitedLocations.Any() && animal.VisitedLocations.Last().LocationPointId == pointId)
            return BadRequest("Animal cannot move to the current point");

        var newLocation = _context.Locations.SingleOrDefault(location => location.Id == pointId);
        if (newLocation == null) return NotFound("Location not found");

        var newRelationship = new AnimalAndLocationRelationship
        {
            Id = animal.VisitedLocations.Any()
                ? animal.VisitedLocations.Select(location => location.Id).Max() + 1
                : 1,
            AnimalId = animalId,
            LocationPointId = pointId,
            Animal = animal,
            Location = newLocation
        };
        newRelationship.InitializeRelationship();

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<AnimalLocationDto>(newRelationship));
    }

    [HttpPut("{animalId:long}/locations")]
    [Authorize]
    public async Task<IActionResult> SetLocation(long animalId, [FromBody] AnimalLocationUpdateRequest request)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");
        if (!request.IsValid())
            return BadRequest("Some field is invalid");
        var animal = _context.Animals
            .Include(animal=>animal.VisitedLocations)
            .Include(animal=>animal.ChippingLocation)
            .SingleOrDefault(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var locationTriplet = animal.VisitedLocations
            .FindWithNextAndPrevious(relationship => relationship.Id == request.VisitedLocationPointId);
        if (locationTriplet == null) return NotFound("Cannot find location with this visitedLocationPointId");

        var oldRelationship = locationTriplet.Value.Current;
        var locationsToCheck = new[]
        {
            locationTriplet.Value.Previous == null ? animal.ChippingLocation : locationTriplet.Value.Previous.Location,
            locationTriplet.Value.Current.Location,
            locationTriplet.Value.Next?.Location
        }.NotNull();
        if (locationsToCheck.Any(location => location.Id == request.LocationPointId))
            return BadRequest("New location id correlates either to its neighbors or to the old location id");

        var newLocation =
            await _context.Locations.FirstOrDefaultAsync(location => location.Id == request.LocationPointId);
        if (newLocation == null)
            return NotFound("Cannot find the new location");

        oldRelationship.ChangeLocationTo(newLocation);

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<AnimalLocationDto>(oldRelationship));
    }


    [HttpDelete("{animalId:long}/locations/{visitedPointId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteLocation(long animalId, long visitedPointId)
    {
        if (animalId <= 0 || visitedPointId <= 0)
            return BadRequest("Id must be positive");
        var animal = await _context.Animals
            .Include(animal=>animal.VisitedLocations)
            .Include(animal=>animal.ChippingLocation)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");


        var locationRelationship = animal.VisitedLocations
            .FirstOrDefault(relationship => relationship.Id == visitedPointId);
        if (locationRelationship == null) return NotFound("Cannot find location with this visitedLocationPointId");

        locationRelationship.Remove(_context);

        if (animal.VisitedLocations.FirstOrDefault() is { } firstLocationRelationship &&
            firstLocationRelationship.Location == animal.ChippingLocation)
            firstLocationRelationship.Remove(_context);

        await _context.SaveChangesAsync();

        return Ok();
    }
}