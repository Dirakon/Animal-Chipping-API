using AutoMapper;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Requests;
using NetTopologySuite.Geometries;

namespace ItPlanetAPI.Models;

public class Area
{
    private Polygon? _polygonRepresentation;

    public Area()
    {
        AreaPoints = new List<AreaPoint>();
    }

    public long Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<AreaPoint> AreaPoints { get; set; }

    public Polygon AsPolygon()
    {
        return _polygonRepresentation ??= AreaPoints.Append(AreaPoints.First()).AsPolygon();
    }

    public IEnumerable<LineSegment> AsSegments()
    {
        var shell = AsPolygon().Shell;
        var previousPoint = shell.StartPoint.Coordinate;
        for (var pointIndex = 1; pointIndex < shell.NumPoints; ++pointIndex)
        {
            var newPoint = shell.GetPointN(pointIndex).Coordinate;
            yield return new LineSegment(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }

    public bool Contains(ISpatial spatial)
    {
        return AsPolygon().Contains(new Point(spatial.AsCoordinate()));
    }

    public bool ContainsOrOnBoundary(ISpatial spatial)
    {
        return AsPolygon().Covers(new Point(spatial.AsCoordinate()));
    }


    public bool IntersectsWith(Area otherArea)
    {
        return AsPolygon().ContainsSomeOf(otherArea.AsPolygon());
    }


    public static async Task<Area> CreateFrom(AreaRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {
        var area = new Area();
        databaseContext.Areas.Add(area);

        await area.TakeValuesOf(request, mapper, databaseContext);

        return area;
    }

    public async Task TakeValuesOf(AreaRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {
        foreach (var areaPoint in AreaPoints) databaseContext.Remove(areaPoint);

        mapper.Map(request, this);
        var areaPointsRequests = request.AreaPoints;
        foreach (var areaPoints in await areaPointsRequests.SelectAsync(areaPointRequest =>
                     AreaPoint.CreateFrom(areaPointRequest, mapper, databaseContext, this)))
            AreaPoints.Add(areaPoints);
    }
}