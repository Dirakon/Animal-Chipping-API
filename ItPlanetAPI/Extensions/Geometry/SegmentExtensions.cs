using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace ItPlanetAPI.Extensions.Geometry;

public static class SegmentExtensions
{
    public static bool Includes(this LineSegment segment2d, Coordinate somePoint)
    {
        if (segment2d.P0.Equals2D(somePoint, MathExtensions.StandardMathEpsilon) ||
            segment2d.P1.Equals2D(somePoint, MathExtensions.StandardMathEpsilon))
            return true;
        var dirToP0 = somePoint.DirectionTo(segment2d.P0);
        var dirToP1 = somePoint.DirectionTo(segment2d.P1);
        var pointBetweenThem = dirToP0.EpsilonEquals(-dirToP1);
        return pointBetweenThem;
    }

    public static Vector2D Direction(this LineSegment segment)
    {
        return segment.P1.DirectionTo(segment.P0);
    }

    public static bool ClosedShapeSelfIntersects(this IEnumerable<LineSegment> segments)
    {
        var segment2ds = segments as LineSegment[] ?? segments.ToArray();
        for (var seg1Index = 0; seg1Index < segment2ds.Length; seg1Index++)
        {
            var seg1 = segment2ds[seg1Index];
            for (var seg2Index = seg1Index + 1; seg2Index < segment2ds.Length; seg2Index++)
            {
                var seg2 = segment2ds[seg2Index];
                if (seg2Index == seg1Index + 1)
                {
                    // If the next segment goes into the previousOne
                    if (seg2.Direction().EpsilonEquals(-seg1.Direction()))
                        return true;
                    continue;
                }

                if (seg1Index == 0 && seg2Index == segment2ds.Length - 1)
                {
                    // If the first segment goes into the last one
                    if (seg2.Direction().EpsilonEquals(-seg1.Direction()))
                        return true;
                    continue;
                }

                if (seg1.Intersection(seg2) != null)
                    return true;
            }
        }

        return false;
    }
}