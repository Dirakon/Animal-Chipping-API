using AutoMapper;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Models;
using ItPlanetAPI.Relationships;
using ItPlanetAPI.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[Route("Animals/{animalId:long}/Types")]
public class TypesOnAnimalController : BaseEntityController
{
    public TypesOnAnimalController(ILogger<TypesOnAnimalController> logger, DatabaseContext context, IMapper mapper) :
        base(logger, context, mapper)
    {
    }


    [HttpPost("{typeId:long}")]
    [Authorize]
    public async Task<IActionResult> Create(long typeId, long animalId)
    {
        if (typeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.AnimalTypes)
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
        return new ObjectResult(_mapper.Map<AnimalDto>(animal)) {StatusCode = StatusCodes.Status201Created};
    }

    [HttpPut("")]
    [Authorize]
    public async Task<IActionResult> Update(long animalId, [FromBody] AnimalTypeUpdateRequest updateRequest)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.AnimalTypes)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var oldTypeRelationship = animal.AnimalTypes.SingleOrDefault(type => type.TypeId == updateRequest.OldTypeId);
        if (oldTypeRelationship == null) return NotFound("Old type is not present on animal");

        if (animal.AnimalTypes.Any(type => type.TypeId == updateRequest.NewTypeId))
            return Conflict("Animal already has the new type");

        var newType =
            await _context.AnimalTypes.FirstOrDefaultAsync(animalType => animalType.Id == updateRequest.NewTypeId);
        if (newType == null) return NotFound("New type cannot be found in the database");

        oldTypeRelationship.ChangeTypeTo(updateRequest.NewTypeId);

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AnimalDto>(animal));
    }

    [HttpDelete("{typeId:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long animalId, long typeId)
    {
        if (typeId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.AnimalTypes)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var typeRelationship = animal.AnimalTypes.SingleOrDefault(type => type.TypeId == typeId);
        if (typeRelationship == null) return NotFound("The type is not present on animal");

        if (animal.AnimalTypes.Count == 1) return BadRequest("The type is the only type the animal has.");

        typeRelationship.Remove(_context);

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AnimalDto>(animal));
    }
}