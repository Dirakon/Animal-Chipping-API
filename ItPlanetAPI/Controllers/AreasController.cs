using AutoMapper;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Middleware.ValidationAttributes;
using ItPlanetAPI.Models;
using ItPlanetAPI.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

[Route("[controller]")]
public class AreasController : BaseEntityController
{
    public AreasController(ILogger<AreasController> logger, DatabaseContext context, IMapper mapper) : base(
        logger, context, mapper)
    {
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get([Positive] long id)
    {
        var areaLookedFor = await _context.Areas.Include(area => area.AreaPoints)
            .SingleOrDefaultAsync(area => area.Id == id);
        if (areaLookedFor == null)
            return NotFound("Area with this id cannot be fount.");

        return Ok(_mapper.Map<AreaDto>(areaLookedFor));
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = nameof(AccountRole.Admin))]
    public async Task<IActionResult> Update([Positive] long id, [FromBody] AreaRequest areaRequest)
    {
        var otherAreas = await _context.Areas
            .Include(area => area.AreaPoints)
            .Where(otherArea => otherArea.Id != id)
            .ToListAsync();

        if (FindProblemWithNewAreaAddition(areaRequest, otherAreas) is { } someProblem)
            return someProblem;

        var oldArea = await _context.Areas.Include(area => area.AreaPoints)
            .SingleOrDefaultAsync(area => area.Id == id);
        if (oldArea == null)
            return NotFound("Area with this id cannot be fount.");

        await oldArea.TakeValuesOf(areaRequest, _mapper, _context);

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AreaDto>(oldArea));
    }

    private IActionResult? FindProblemWithNewAreaAddition(AreaRequest areaToPotentiallyAdd, List<Area> otherAreas)
    {
        var newAreaPoints = areaToPotentiallyAdd.AreaPoints;
        var newAreaPolygon = newAreaPoints.Append(newAreaPoints.First()).AsPolygon();

        if (!newAreaPolygon.IsValid)
            return BadRequest("Topologically invalid, according to the OGC SFS specification");

        if (otherAreas.Any(otherArea => otherArea.Name == areaToPotentiallyAdd.Name))
            return Conflict("Area with this name already exists");

        // TODO: change logic if BadRequest is expected when the points are the same but in different order
        if (otherAreas.Any(otherArea => otherArea.AreaPoints.Count == newAreaPoints.Count
                                        && otherArea.AreaPoints.All(otherAreaPoint =>
                                            newAreaPoints.Any(thisAreaPoint =>
                                                thisAreaPoint.IsAlmostTheSameAs(otherAreaPoint)))))
            return Conflict("Area with the same points has been found");

        if (otherAreas.FirstOrDefault(otherArea => otherArea.AsPolygon().ContainsSomeOf(newAreaPolygon)) is
            { } someArea)
            return BadRequest($"The area intersects with another existing area (id: {someArea.Id})");

        return null;
    }

    [HttpPost("")]
    [Authorize(Roles = nameof(AccountRole.Admin))]
    public async Task<IActionResult> Create([FromBody] AreaRequest areaRequest)
    {
        var otherAreas = await _context.Areas
            .Include(area => area.AreaPoints)
            .ToListAsync();

        if (FindProblemWithNewAreaAddition(areaRequest, otherAreas) is { } someProblem)
            return someProblem;
        var thisArea = await Area.CreateFrom(areaRequest, _mapper, _context);

        await _context.SaveChangesAsync();
        return new ObjectResult(_mapper.Map<AreaDto>(thisArea))
            {StatusCode = StatusCodes.Status201Created};
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = nameof(AccountRole.Admin))]
    public async Task<IActionResult> Delete([Positive] long id)
    {
        var area = await _context.Areas.Include(area => area.AreaPoints)
            .SingleOrDefaultAsync(area => area.Id == id);
        if (area == null)
            return NotFound("Area with this id cannot be found.");

        foreach (var areaPoint in area.AreaPoints) _context.Remove(areaPoint);
        _context.Areas.Remove(area);

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{id:long}/analytics")]
    [Authorize]
    public async Task<IActionResult> Analytics([Positive] long id, [FromQuery] DateTimeOffset startDate,
        [FromQuery] DateTimeOffset endDate)
    {
        if (startDate >= endDate)
            return BadRequest("Start date should be earlier then end date");

        var area = await _context.Areas.Include(area => area.AreaPoints)
            .SingleOrDefaultAsync(area => area.Id == id);
        if (area == null)
            return NotFound("Area with this id cannot be found.");

        var animals = await _context.Animals
            .Include(animal => animal.AnimalTypes)
            .ThenInclude(typeRelationship => typeRelationship.Type)
            .Include(animal => animal.ChippingLocation)
            .Include(animal => animal.VisitedLocations)
            .ThenInclude(locationRelationship => locationRelationship.Location)
            .ToListAsync();
        AreaAnalytics analytics = new(area, startDate, endDate);
        foreach (var animal in animals) analytics.AnalyzeAnimalMovements(animal);

        return Ok(_mapper.Map<AreaAnalyticsDto>(analytics));
    }
}