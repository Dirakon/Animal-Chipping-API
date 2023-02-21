using AutoMapper;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Models;
using ItPlanetAPI.Relationships;
using ItPlanetAPI.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[Route("Animals/{animalId:long}/Locations")]
public class AnimalLocationsController : BaseEntityController
{
    public AnimalLocationsController(ILogger<AnimalLocationsController> logger, DatabaseContext context, IMapper mapper)
        : base(logger, context, mapper)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Get(long animalId, [FromQuery] AnimalLocationSearchRequest request)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        return Ok(animal.VisitedLocations
            .Where(locationRelationship => locationRelationship.DateTimeOfVisitLocationPoint >= request.StartDateTime
                                           && locationRelationship.DateTimeOfVisitLocationPoint <= request.EndDateTime)
            .OrderBy(locationRelationship => locationRelationship.DateTimeOfVisitLocationPoint)
            .Skip(request.From)
            .Take(request.Size)
            .Select(locationRelationship => _mapper.Map<AnimalLocationDto>(locationRelationship))
        );
    }

    [HttpPost("{pointId:long}")]
    [Authorize]
    public async Task<IActionResult> Create(long animalId, long pointId)
    {
        if (pointId <= 0 || animalId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.VisitedLocations)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        if (animal.LifeStatus == AnimalLifeStatus.Dead) return BadRequest("Cannot move dead animal.");
        if (pointId == animal.ChippingLocationId && animal.VisitedLocations.IsEmpty())
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
        return new ObjectResult(_mapper.Map<AnimalLocationDto>(newRelationship))
            {StatusCode = StatusCodes.Status201Created};
    }

    [HttpPut("")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] AnimalLocationUpdateRequest request, long animalId)
    {
        if (animalId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.VisitedLocations)
            .Include(animal => animal.ChippingLocation)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var locationTriplet = animal.VisitedLocations
            .FindWithNextAndPrevious(relationship => relationship.Id == request.VisitedLocationPointId);
        if (locationTriplet == null) return NotFound("Cannot find location with this visitedLocationPointId");

        var oldRelationship = locationTriplet.Value.Current;
        var locationsToCheck = new[]
        {
            locationTriplet.Value.Previous == null
                ? animal.ChippingLocationId
                : locationTriplet.Value.Previous.LocationPointId,
            locationTriplet.Value.Current.LocationPointId,
            locationTriplet.Value.Next?.LocationPointId
        }.NotNull();
        if (locationsToCheck.Any(locationId => locationId == request.LocationPointId))
            return BadRequest("New location id correlates either to its neighbors or to the old location id");

        var newLocation =
            await _context.Locations.FirstOrDefaultAsync(location => location.Id == request.LocationPointId);
        if (newLocation == null)
            return NotFound("Cannot find the new location");

        oldRelationship.ChangeLocationTo(request.LocationPointId);

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AnimalLocationDto>(oldRelationship));
    }


    [HttpDelete("{visitedPointId:long}")]
    [Authorize]
    public async Task<IActionResult> Delete(long animalId, long visitedPointId)
    {
        if (animalId <= 0 || visitedPointId <= 0)
            return BadRequest("Id must be positive");

        var animal = await _context.Animals
            .Include(animal => animal.VisitedLocations)
            .Include(animal => animal.ChippingLocation)
            .FirstOrDefaultAsync(animal => animal.Id == animalId);
        if (animal == null) return NotFound("Animal not found");

        var locationRelationship = animal.VisitedLocations
            .FirstOrDefault(relationship => relationship.Id == visitedPointId);
        if (locationRelationship == null) return NotFound("Cannot find location with this visitedLocationPointId");

        locationRelationship.Remove(_context);
        if (animal.VisitedLocations.FirstOrDefault(relationship => relationship.Id != visitedPointId) is
                { } firstLocationRelationship &&
            firstLocationRelationship.LocationPointId == animal.ChippingLocationId)
            firstLocationRelationship.Remove(_context);

        await _context.SaveChangesAsync();
        return Ok();
    }
}