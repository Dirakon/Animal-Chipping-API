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
public class AnimalsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<AnimalsController> _logger;

    private readonly IMapper _mapper;

    public AnimalsController(ILogger<AnimalsController> logger, DatabaseContext context, IMapper mapper)
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

            return new ObjectResult(_mapper.Map<AnimalDto>(animal)) { StatusCode = StatusCodes.Status201Created };
        }

        return NotFound("Some entities in request cannot be found");
    }

    [HttpDelete("{animalId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteAnimal(long animalId)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = await _context.Animals
            .Include(animal=>animal.AnimalTypes)
            .Include(animal=>animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");
        if (animal.VisitedLocations.Any())
        {
            return BadRequest("Animal left chipping location and it has some visited location points.");
        }
        foreach (var animalAndTypeRelationship in animal.AnimalTypes)
        {
            animalAndTypeRelationship.Remove(_context);
        }

        _context.Animals.Remove(animal);

        await _context.SaveChangesAsync();
        return Ok();
    }



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

        var newType = await _context.AnimalTypes.FirstOrDefaultAsync(type => type.Id == typeId);
        if (newType == null) return NotFound("Type not found");

        var newRelationship = new AnimalAndTypeRelationship
            {Animal = animal, AnimalId = animalId, Type = newType, TypeId = typeId};

        newRelationship.InitializeRelationship();

        await _context.SaveChangesAsync();

        return  new ObjectResult(_mapper.Map<AnimalDto>(animal)) { StatusCode = StatusCodes.Status201Created };
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

        var newType = await _context.AnimalTypes.FirstOrDefaultAsync(animalType => animalType.Id == updateRequest.NewTypeId);
        if (newType == null) return NotFound("New type cannot be found in the database");

        oldTypeRelationship.ChangeTypeTo(updateRequest.NewTypeId);
        
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
    [HttpGet("{animalId:long}/locations/")]
    [ForbidOnIncorrectAuthorizationHeader]
    public async Task<IActionResult> GetLocation(long animalId, [FromQuery] AnimalLocationSearchRequest request)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");
        var animal = await _context.Animals
            .Include(animal=>animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");


        return Ok(animal.VisitedLocations
            .Where(locationRelationship=>locationRelationship.DateTimeOfVisitLocationPoint >= request.StartDateTime 
                                         &&  locationRelationship.DateTimeOfVisitLocationPoint <= request.EndDateTime)
            .OrderBy(locationRelationship=>locationRelationship.DateTimeOfVisitLocationPoint)
            .Skip(request.From)
            .Take(request.Size)
            .Select(locationRelationship=>_mapper.Map<AnimalLocationDto>(locationRelationship))
        );
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

        var newLocation = await _context.Locations.FirstOrDefaultAsync(location => location.Id == pointId);
        if (newLocation == null) return NotFound("Location not found");

        var newRelationship = new AnimalAndLocationRelationship
        {
            AnimalId = animalId,
            LocationPointId = pointId,
            Animal = animal,
            Location = newLocation
        };
        
        newRelationship.InitializeRelationship();

        await _context.SaveChangesAsync();

        return new ObjectResult(_mapper.Map<AnimalLocationDto>(newRelationship)) { StatusCode = StatusCodes.Status201Created };
    }

    [HttpPut("{animalId:long}/locations")]
    [Authorize]
    public async Task<IActionResult> SetLocation(long animalId, [FromBody] AnimalLocationUpdateRequest request)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");
        if (!request.IsValid())
            return BadRequest("Some field is invalid");
        var animal = await _context.Animals
            .Include(animal=>animal.VisitedLocations)
            .Include(animal=>animal.ChippingLocation)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var locationTriplet = animal.VisitedLocations
            .FindWithNextAndPrevious(relationship => relationship.Id == request.VisitedLocationPointId);
        if (locationTriplet == null) return NotFound("Cannot find location with this visitedLocationPointId");

        var oldRelationship = locationTriplet.Value.Current;
        var locationsToCheck = new[]
        {
            locationTriplet.Value.Previous == null ? animal.ChippingLocationId : locationTriplet.Value.Previous.LocationPointId,
            locationTriplet.Value.Current.LocationPointId,
            locationTriplet.Value.Next?.LocationPointId
        }.NotNull();
        if (locationsToCheck.Any(locationId => locationId == request.LocationPointId))
            return BadRequest("New location id correlates either to its neighbors or to the old location id");

        var newLocation =
            await _context.Locations.FirstOrDefaultAsync(location => location.Id == request.LocationPointId);
        if (newLocation == null)
            return NotFound("Cannot find the new location");

        oldRelationship.ChangeLocationTo( request.LocationPointId);

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

        if (animal.VisitedLocations.FirstOrDefault(relationship => relationship.Id!=visitedPointId) is { } firstLocationRelationship &&
            firstLocationRelationship.LocationPointId == animal.ChippingLocationId)
            firstLocationRelationship.Remove(_context);

        await _context.SaveChangesAsync();

        return Ok();
    }
}