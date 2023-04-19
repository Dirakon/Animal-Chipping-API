using NetTopologySuite.Geometries;

namespace ItPlanetAPI.Extensions.Geometry;

public static class PolygonExtensions
{
    public static bool ContainsSomeOf(this Polygon polygon1, Polygon polygon2)
    {
        return polygon1.Overlaps(polygon2) || polygon1.Covers(polygon2) || polygon2.Covers(polygon1);

        // Alternative version:
        // var intersectionShape = polygon1.Intersection(polygon2);
        // return !intersectionShape.IsEmpty && intersectionShape.Dimension == Dimension.Surface;
        // TODO: test which one is more performant
    }
}