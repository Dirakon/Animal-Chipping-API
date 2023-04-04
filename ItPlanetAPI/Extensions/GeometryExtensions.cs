using g3;
using ItPlanetAPI.Models;
using ISpatial = ItPlanetAPI.Models.ISpatial;

namespace ItPlanetAPI.Extensions;

public static class GeometryExtensions
{
    public static IEnumerable<Segment2d> AsSegments(this IEnumerable<ISpatial> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            yield break;
        var firstPoint = enumerator.Current.AsVector();
        var previousPoint = firstPoint;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current.AsVector();
            yield return new Segment2d(previousPoint, somePoint);
            previousPoint = somePoint;
        }

        if (!firstPoint.EpsilonEqual(previousPoint, 0.0001)) yield return new Segment2d(previousPoint, firstPoint);
    }

    public static bool Includes(this Segment2d segment2d, Vector2d somePoint)
    {
        if (segment2d.P0.EpsilonEqual(somePoint, 0.0001) || segment2d.P1.EpsilonEqual(somePoint, 0.0001))
            return true;
        var dirToP0 = (segment2d.P0 - somePoint).Normalized;
        var dirToP1 = (segment2d.P1 - somePoint).Normalized;
        var pointBetweenThem = dirToP0.EpsilonEqual(-dirToP1, 0.0001);
        return pointBetweenThem;
    }

    public static bool ClosedShapeSelfIntersects(this IEnumerable<Segment2d> segments)
    {
        var segment2ds = segments as Segment2d[] ?? segments.ToArray();
        for (var seg1Index = 0; seg1Index < segment2ds.Length; seg1Index++)
        {
            var seg1 = segment2ds[seg1Index];
            for (var seg2Index = seg1Index + 1; seg2Index < segment2ds.Length; seg2Index++)
            {
                var seg2 = segment2ds[seg2Index];
                if (seg2Index == seg1Index + 1)
                {
                    // If the next segment goes into the previousOne
                    if (seg2.Direction.EpsilonEqual(-seg1.Direction, 0.001))
                        return true;
                    continue;
                }

                if (seg1Index == 0 && seg2Index == segment2ds.Length - 1)
                {
                    // If the first segment goes into the last one
                    if (seg2.Direction.EpsilonEqual(-seg1.Direction, 0.001))
                        return true;
                    continue;
                }

                if (seg1.Intersects(seg2))
                    return true;
            }
        }

        return false;
    }

    public static bool IntersectsNonBoundaryWise(this Polygon2d polygon1, Polygon2d polygon2)
    {
        if (!polygon1.GetBounds().Intersects(polygon2.GetBounds()))
            return false;
        foreach (var seg1 in polygon1.SegmentItr())
        foreach (var seg2 in polygon2.SegmentItr())
        {
            if (!seg1.Intersects(seg2)) continue;

            // TODO: fix overlooked problem when two segments one by one go through the polygon bounds
            // by intersecting "boundary-wise" from different sides
            var isIntersectionBoundaryWise = new[] {seg1.P0, seg1.P1, seg2.P0}.AreOnOneLine() ||
                                             new[] {seg1.P0, seg1.P1, seg2.P1}.AreOnOneLine();
            if (!isIntersectionBoundaryWise)
                return true;
        }

        return false;
    }


    public static Polygon2d AsPolygon(this IEnumerable<ISpatial> points)
    {
        return new Polygon2d(points.Select(point => point.AsVector()));
    }

    public static bool EpsilonEqual(this Segment2d seg1, Segment2d seg2, double epsilon)
    {
        return seg1.P0.EpsilonEqual(seg2.P0, epsilon) && seg1.P1.EpsilonEqual(seg2.P1, epsilon);
    }

    public static bool AreOnOneLine(this IEnumerable<Vector2d> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            return false;
        var firstPoint = enumerator.Current;
        Vector2d? firstNonZeroDirection = null;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current;
            var direction = somePoint - firstPoint;
            if (direction.EpsilonEqual(Vector2d.Zero, 0.001))
                continue;
            direction = direction.Normalized;
            if (firstNonZeroDirection is { } firstDirection)
            {
                if (!direction.EpsilonEqual(firstDirection, 0.0001) && !direction.EpsilonEqual(-firstDirection, 0.0001))
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
        return points.Select(point => point.AsVector()).AreOnOneLine();
    }
}