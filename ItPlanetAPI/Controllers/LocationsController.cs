using AutoMapper;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Middleware.ValidationAttributes;
using ItPlanetAPI.Models;
using ItPlanetAPI.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[Route("[controller]")]
public class LocationsController : BaseEntityController
{
    public LocationsController(ILogger<LocationsController> logger, DatabaseContext context, IMapper mapper) : base(
        logger, context, mapper)
    {
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get([Positive] long id)
    {
        var locationSearchedFor =
            await _context.Locations.FirstOrDefaultAsync(animalLocation => animalLocation.Id == id);

        return locationSearchedFor switch
        {
            { } location => Ok(_mapper.Map<LocationDto>(location)),
            null => NotFound("Location with this id is not found")
        };
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Update([Positive] long id, [FromBody] LocationRequest locationRequest)
    {
        var oldLocation =
            await _context.Locations.SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldLocation == null)
            return NotFound();

        var coordinatesAlreadyPresent =
            await _context.Locations.AnyAsync(ISpatialExtensions.IsAlmostTheSameAs(locationRequest));
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        _mapper.Map(locationRequest, oldLocation);

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<LocationDto>(oldLocation));
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] LocationRequest locationRequest)
    {
        var coordinatesAlreadyPresent =
            await _context.Locations.AnyAsync(ISpatialExtensions.IsAlmostTheSameAs(locationRequest));
        if (coordinatesAlreadyPresent) return Conflict("Location with these coordinates is already present");

        var location = _mapper.Map<Location>(locationRequest);
        _context.Locations.Add(location);

        await _context.SaveChangesAsync();
        return new ObjectResult(_mapper.Map<LocationDto>(location)) {StatusCode = StatusCodes.Status201Created};
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete([Positive] long id)
    {
        var oldLocation =
            await _context.Locations
                .Include(location => location.AnimalsChippedHere)
                .Include(location => location.AnimalsVisitedHere)
                .SingleOrDefaultAsync(animalLocation => animalLocation.Id == id);
        if (oldLocation == null)
            return NotFound();

        var locationIsPresentInAnimals =
            oldLocation.AnimalsChippedHere.Any() || oldLocation.AnimalsVisitedHere.Any();
        if (locationIsPresentInAnimals) return BadRequest("Animal connected with this location is present");

        _context.Locations.Remove(oldLocation);

        await _context.SaveChangesAsync();
        return Ok();
    }
}