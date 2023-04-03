using AutoMapper;
using g3;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Requests;

namespace ItPlanetAPI.Models;

public class Area
{
    private Polygon2d? _polygonRepresentation;

    public Area()
    {
        AreaPoints = new List<AreaPoint>();
    }

    public long Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<AreaPoint> AreaPoints { get; set; }

    public Polygon2d AsPolygon()
    {
        return _polygonRepresentation ??= AreaPoints.AsPolygon();
    }

    public IEnumerable<Segment2d> AsSegments()
    {
        return AsPolygon().SegmentItr();
    }

    public bool Contains(ISpatial spatial)
    {
        return AsPolygon().Contains(spatial.AsVector());
    }

    public bool IntersectsWith(Area otherArea)
    {
        return AsPolygon().IntersectsNonBoundaryWise(otherArea.AsPolygon());
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