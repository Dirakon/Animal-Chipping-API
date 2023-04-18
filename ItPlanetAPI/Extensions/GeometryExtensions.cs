using ItPlanetAPI.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace ItPlanetAPI.Extensions;

public static class GeometryExtensions
{
    public const double StandardGeometryEpsilon = 0.0001;

    public static IEnumerable<LineSegment> AsSegments(this IEnumerable<ISpatial> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            yield break;
        var firstPoint = enumerator.Current.AsCoordinate();
        var previousPoint = firstPoint;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current.AsCoordinate();
            yield return new LineSegment(previousPoint, somePoint);
            previousPoint = somePoint;
        }

        if (!firstPoint.Equals2D(previousPoint, StandardGeometryEpsilon))
            yield return new LineSegment(previousPoint, firstPoint);
    }


    public static bool Includes(this LineSegment segment2d, Coordinate somePoint)
    {
        if (segment2d.P0.Equals2D(somePoint, StandardGeometryEpsilon) ||
            segment2d.P1.Equals2D(somePoint, StandardGeometryEpsilon))
            return true;
        var dirToP0 = somePoint.DirectionTo(segment2d.P0);
        var dirToP1 = somePoint.DirectionTo(segment2d.P1);
        var pointBetweenThem = dirToP0.EpsilonEquals(-dirToP1);
        return pointBetweenThem;
    }

    public static Vector2D DirectionTo(this Coordinate coordinate1, Coordinate coordinate2)
    {
        return (new Vector2D(coordinate2) - new Vector2D(coordinate1)).Normalize();
    }

    public static Vector2D Direction(this LineSegment segment)
    {
        return segment.P1.DirectionTo(segment.P0);
    }

    public static bool EpsilonEquals(this Vector2D vector1, Vector2D vector2, double epsilon = StandardGeometryEpsilon)
    {
        return vector1.Distance(vector2) <= epsilon;
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

    public static bool ContainsSomeOf(this Polygon polygon1, Polygon polygon2)
    {
        return polygon1.Overlaps(polygon2) || polygon1.Covers(polygon2) || polygon2.Covers(polygon1);

        // Alternative version:
        // var intersectionShape = polygon1.Intersection(polygon2);
        // return !intersectionShape.IsEmpty && intersectionShape.Dimension == Dimension.Surface;
        // TODO: test which one is more performant
    }

    /**
     * Transforms an IEnumerable of Spatial into a polygon.
     * Note that the original Spatial list should be closed (i.e. the first and the last items should be identical)
     */
    public static Polygon AsPolygon(this IEnumerable<ISpatial> points)
    {
        return new Polygon(new LinearRing(points.Select(point => point.AsCoordinate()).ToArray()));
    }

    public static bool AreOnOneLine(this IEnumerable<Coordinate> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            return false;
        var firstPoint = enumerator.Current;
        Vector2D? firstNonZeroDirection = null;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current;
            if (firstPoint.Equals2D(somePoint, StandardGeometryEpsilon))
                continue;
            var direction = somePoint.DirectionTo(firstPoint);
            if (firstNonZeroDirection is { } firstDirection)
            {
                if (!direction.EpsilonEquals(firstDirection) && !direction.EpsilonEquals(-firstDirection))
                    return false;
            }
            else
            {
                firstNonZeroDirection = direction;
            }
        }

        return true;
    }

    public static bool AreOnOneLine(this IEnumerable<ISpatial> points)
    {
        return points.Select(point => point.AsCoordinate()).AreOnOneLine();
    }
}