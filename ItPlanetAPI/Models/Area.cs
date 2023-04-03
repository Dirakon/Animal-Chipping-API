using g3;
using ItPlanetAPI.Extensions;

namespace ItPlanetAPI.Models;

public class Area
{
    public Area()
    {
        AreaPoints = new List<AreaPoint>();
    }
    
    public long Id { get; set; }
    public string Name { get; set; }
    
    public virtual ICollection<AreaPoint> AreaPoints { get; set; }
    private Polygon2d? _polygonRepresentation = null;
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
        return AsPolygon().Intersects(otherArea.AsPolygon());
    }
}