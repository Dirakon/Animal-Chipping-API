using AutoMapper;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Middleware.ValidationAttributes;
using ItPlanetAPI.Models;
using ItPlanetAPI.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[Route("Animals/Types")]
public class AnimalTypeController : BaseEntityController
{
    public AnimalTypeController(ILogger<AnimalTypeController> logger, DatabaseContext context, IMapper mapper) : base(
        logger, context, mapper)
    {
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get([Positive] long id)
    {
        var animalTypeSearchedFor = await _context.AnimalTypes.FirstOrDefaultAsync(animalType => animalType.Id == id);

        return animalTypeSearchedFor switch
        {
            { } animalType => Ok(_mapper.Map<AnimalTypeDto>(animalType)),
            null => NotFound("Animal type with this id is not found")
        };
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Update([Positive] long id, [FromBody] AnimalTypeRequest animalTypeRequest)
    {
        var oldAnimalType = await _context.AnimalTypes.SingleOrDefaultAsync(animalType => animalType.Id == id);
        if (oldAnimalType == null)
            return NotFound();

        var newTypeAlreadyPresent = await _context.AnimalTypes.AnyAsync(typeToCheck =>
            typeToCheck.Type == animalTypeRequest.Type && typeToCheck.Id != id);
        if (newTypeAlreadyPresent) return Conflict("Animal type with this 'type' field already present");

        _mapper.Map(animalTypeRequest, oldAnimalType);

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AnimalTypeDto>(oldAnimalType));
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AnimalTypeRequest animalTypeRequest)
    {
        var newTypeAlreadyPresent =
            await _context.AnimalTypes.AnyAsync(typeToCheck => typeToCheck.Type == animalTypeRequest.Type);
        if (newTypeAlreadyPresent) return Conflict("Animal type with this 'type' field already present");

        var animalType = _mapper.Map<AnimalType>(animalTypeRequest);
        _context.AnimalTypes.Add(animalType);

        await _context.SaveChangesAsync();
        return new ObjectResult(_mapper.Map<AnimalTypeDto>(animalType)) {StatusCode = StatusCodes.Status201Created};
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete([Positive] long id)
    {
        var oldAnimalType = await _context.AnimalTypes
            .Include(animalType => animalType.Animals)
            .SingleOrDefaultAsync(animalType => animalType.Id == id);
        if (oldAnimalType == null)
            return NotFound();

        var typeIsPresentInAnimals = oldAnimalType.Animals.Any();
        if (typeIsPresentInAnimals) return BadRequest("Animal with this type is present");

        _context.AnimalTypes.Remove(oldAnimalType);

        await _context.SaveChangesAsync();
        return Ok();
    }
}