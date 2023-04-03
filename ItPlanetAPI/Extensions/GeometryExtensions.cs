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
        Vector2d firstPoint = enumerator.Current.AsVector();
        Vector2d previousPoint = firstPoint;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current.AsVector();
            yield return new Segment2d(previousPoint, somePoint);
            previousPoint = somePoint;
        }

        if (!firstPoint.EpsilonEqual(previousPoint, 0.0001))
        {
            yield return new Segment2d(previousPoint, firstPoint);
        }
    }
    public static bool SelfIntersect(this IEnumerable<Segment2d> segments)
    {
        var segment2ds = segments as Segment2d[] ?? segments.ToArray();
        for (var seg1Index = 0; seg1Index < segment2ds.Length; seg1Index++)
        {
            var seg1 = segment2ds[seg1Index];
            for (var seg2Index = seg1Index+1; seg2Index < segment2ds.Length; seg2Index++)
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
    public static Polygon2d AsPolygon(this IEnumerable<ISpatial> points)
    {
        return new Polygon2d(points.Select(point => point.AsVector()));
    }

    public static bool EpsilonEqual(this Segment2d seg1, Segment2d seg2, double epsilon)
    {
        return seg1.P0.EpsilonEqual(seg2.P0, epsilon) && seg1.P1.EpsilonEqual(seg2.P1, epsilon);
    }

    public static bool AreOnOneLine(this IEnumerable<ISpatial> points)
    {
        using var enumerator = points.GetEnumerator();
        
        if (!enumerator.MoveNext())
            return false;
        Vector2d firstPoint = enumerator.Current.AsVector();
        
        if (!enumerator.MoveNext())
            return false;
        Vector2d firstDirection = (enumerator.Current.AsVector() - firstPoint).Normalized;
        
        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current.AsVector();
            var direction = (somePoint - firstPoint).Normalized;
            if (!direction.EpsilonEqual(firstDirection,0.0001))
                return false;
        }

        return true;
    }
}