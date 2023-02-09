using AutoMapper;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;


[ApiController]
[Route("[controller]")]
public class AnimalTypeController : ControllerBase
{ 
    
    private readonly IMapper _mapper;
    private readonly DatabaseContext _context;
    private readonly ILogger<AnimalTypeController> _logger;

    public AnimalTypeController(ILogger<AnimalTypeController> logger, DatabaseContext context, IMapper mapper)
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

        var animalTypeSearchedFor = _context.AnimalTypes.Find(animalType => animalType.Id == id);

        return animalTypeSearchedFor.Match<IActionResult>(
            Some: Ok,
            None: NotFound("Animal type with this id is not found")
        );
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
        
        var newTypeAlreadyPresent = _context.AnimalTypes.Any(typeToCheck => typeToCheck.Type == animalTypeRequest.Type && typeToCheck.Id != id);
        if (newTypeAlreadyPresent) return Conflict("Animal type with this 'type' field already present");

        _mapper.Map(source: animalTypeRequest, destination: oldAnimalType);
        
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();
        
        return Ok(oldAnimalType);

    }
    
    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] AnimalTypeRequest animalTypeRequest)
    {
        if (!animalTypeRequest.IsValid()) return BadRequest("Some field is invalid");
        
        var newTypeAlreadyPresent = _context.AnimalTypes.Any(typeToCheck => typeToCheck.Type == animalTypeRequest.Type);
        if (newTypeAlreadyPresent) return Conflict("Animal type with this 'type' field already present");

        var animalType = _mapper.Map<AnimalType>(animalTypeRequest);
        if (!_context.AnimalTypes.Any())
            animalType.Id = 1;
        else
            animalType.Id = await _context.AnimalTypes.Select(typeToCheck => typeToCheck.Id).MaxAsync() + 1;

        _context.AnimalTypes.Add(animalType);
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();

        return Ok(animalType);
    }
    
    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long id)
    {
        if (id <= 0) return BadRequest("Id must be positive");
        
        var oldAnimalType = await _context.AnimalTypes.SingleOrDefaultAsync(animalType => animalType.Id == id);
        if (oldAnimalType == null)
            return NotFound();
        
        var typeIsPresentInAnimals = _context.Animals.Any(animal => animal.AnimalTypes.Contains(id));
        if (typeIsPresentInAnimals) return BadRequest("Animal with this type is present");

        _context.AnimalTypes.Remove(oldAnimalType);
        
        // TODO: add await if there is a possibility of user sending a request with their ip before the changes are saved
        _context.SaveChangesAsync();
        
        return Ok();
    }

}