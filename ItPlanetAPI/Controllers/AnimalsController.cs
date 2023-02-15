using AutoMapper;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Models;
using ItPlanetAPI.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[Route("[controller]")]
public class AnimalsController : BaseEntityController
{
    public AnimalsController(ILogger<AnimalsController> logger, DatabaseContext context, IMapper mapper) : base(logger,
        context, mapper)
    {
    }

    [HttpGet("Search")]
    public IActionResult Search([FromQuery] AnimalSearchRequest searchRequest)
    {
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
                .Include(animal => animal.AnimalTypes)
                .Include(animal => animal.VisitedLocations)
                .Select(animal => _mapper.Map<AnimalDto>(animal))
        );
    }


    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var animalSearchedFor = await _context.Animals
            .Include(animal => animal.AnimalTypes)
            .Include(animal => animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == id);

        return animalSearchedFor switch
        {
            null => NotFound("Location with this id is not found"),
            _ => Ok(_mapper.Map<AnimalDto>(animalSearchedFor))
        };
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Update(long id, [FromBody] AnimalUpdateRequest animalRequest)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimal =
            await _context.Animals
                .Include(animal => animal.AnimalTypes)
                .Include(animal => animal.VisitedLocations)
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
    public async Task<IActionResult> Create([FromBody] AnimalCreationRequest animalRequest)
    {
        if (animalRequest.HasConflicts()) return Conflict();

        if (await Animal.TryCreateFrom(animalRequest, _mapper, _context) is { } animal)
        {
            await _context.SaveChangesAsync();

            return new ObjectResult(_mapper.Map<AnimalDto>(animal)) {StatusCode = StatusCodes.Status201Created};
        }

        return NotFound("Some entities in request cannot be found");
    }

    [HttpDelete("{animalId:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long animalId)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.AnimalTypes)
            .Include(animal => animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        if (animal.VisitedLocations.Any())
            return BadRequest("Animal left chipping location and it has some visited location points.");

        foreach (var animalAndTypeRelationship in animal.AnimalTypes) animalAndTypeRelationship.Remove(_context);
        _context.Animals.Remove(animal);

        await _context.SaveChangesAsync();
        return Ok();
    }
}