using AutoMapper;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[ApiController]
[Route("animals/types")]
public class AnimalTypeController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<AnimalTypeController> _logger;

    private readonly IMapper _mapper;

    public AnimalTypeController(ILogger<AnimalTypeController> logger, DatabaseContext context, IMapper mapper)
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

        var animalTypeSearchedFor = await _context.AnimalTypes.FirstOrDefaultAsync(animalType => animalType.Id == id);

        return animalTypeSearchedFor switch
        {
            { } animalType => Ok(_mapper.Map<AnimalTypeDto>(animalType)),
            null => NotFound("Animal type with this id is not found")
        };
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Put(long id, [FromBody] AnimalTypeRequest animalTypeRequest)
    {
        if (!animalTypeRequest.IsValid()) return BadRequest("Some field is invalid");
        if (id <= 0) return BadRequest("Id must be positive");

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
    public async Task<IActionResult> Post([FromBody] AnimalTypeRequest animalTypeRequest)
    {
        if (!animalTypeRequest.IsValid()) return BadRequest("Some field is invalid");

        var newTypeAlreadyPresent =await _context.AnimalTypes.AnyAsync(typeToCheck => typeToCheck.Type == animalTypeRequest.Type);
        if (newTypeAlreadyPresent) return Conflict("Animal type with this 'type' field already present");

        var animalType = _mapper.Map<AnimalType>(animalTypeRequest);

        _context.AnimalTypes.Add(animalType);
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<AnimalTypeDto>(animalType));
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");

        var oldAnimalType = await _context.AnimalTypes
            .Include(animalType=>animalType.Animals)
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