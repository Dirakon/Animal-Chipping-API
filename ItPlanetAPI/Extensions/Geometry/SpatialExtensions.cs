using System.Linq.Expressions;
using ItPlanetAPI.Models;
using NetTopologySuite.Geometries;

namespace ItPlanetAPI.Extensions.Geometry;

public static class SpatialExtensions
{
    public static bool IsAlmostTheSameAs(this ISpatial first, ISpatial second)
    {
        return IsAlmostTheSameAs(second).Compile().Invoke(first);
    }

    public static Coordinate AsCoordinate(this ISpatial spatial)
    {
        return new Coordinate(spatial.Latitude, spatial.Longitude);
    }

    public static Point AsPoint(this ISpatial spatial)
    {
        return new Point(spatial.AsCoordinate());
    }


    public static Expression<Func<ISpatial, bool>> IsAlmostTheSameAs(ISpatial other)
    {
        const double epsilon = MathExtensions.StandardMathEpsilon;
        return current => other.Latitude - current.Latitude < epsilon &&
                          -other.Latitude + current.Latitude < epsilon &&
                          other.Longitude - current.Longitude < epsilon &&
                          -other.Longitude + current.Longitude < epsilon;
    }

    /**
     * Transforms an IEnumerable of Spatial into a polygon.
     * Note that the original Spatial collection should be closed (i.e. the first and the last items should be identical)
     */
    public static Polygon AsPolygon(this IEnumerable<ISpatial> points)
    {
        return new Polygon(new LinearRing(points.Select(point => point.AsCoordinate()).ToArray()));
    }

    public static bool AreOnOneLine(this IEnumerable<ISpatial> points)
    {
        return points.Select(point => point.AsCoordinate()).AreOnOneLine();
    }

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

        if (!firstPoint.Equals2D(previousPoint, MathExtensions.StandardMathEpsilon))
            yield return new LineSegment(previousPoint, firstPoint);
    }
    
    public static bool DescribesTheSamePolygonAs(this IEnumerable<ISpatial> points1Source,
        IEnumerable<ISpatial> points2Source)
    {
        var points2 = points2Source.ToList();

        int? polygon2Index = null;
        int? polygon2Direction = null;
        foreach (var poly1Point in points1Source)
        {
            if (polygon2Index is not { } someIndex)
            {
                polygon2Index = points2.FindIndex(poly2Point => poly1Point.IsAlmostTheSameAs(poly2Point));
                if (polygon2Index == -1)
                    return false;
                continue;
            }

            var nextPolygon2PointIndex = someIndex == points2.Count - 1 ? 0 : someIndex + 1;
            var previousPolygon2PointIndex = someIndex == 0 ? points2.Count - 1 : someIndex - 1;

            if (polygon2Direction is not { } someDirection)
            {
                /*
                * Note that when deciding the direction,
                * we ignore the situation when there are equivalent points in both directions
                * because it would mean that the shape is self-intersecting, which is not allowed.
                */
                if (poly1Point.IsAlmostTheSameAs(points2[nextPolygon2PointIndex]))
                {
                    polygon2Index = nextPolygon2PointIndex;
                    polygon2Direction = 1;
                }
                else if (poly1Point.IsAlmostTheSameAs(points2[previousPolygon2PointIndex]))
                {
                    polygon2Index = previousPolygon2PointIndex;
                    polygon2Direction = -1;
                }
                else
                {
                    return false;
                }

                continue;
            }

            var poly2PointIndex = someDirection == 1 ? nextPolygon2PointIndex : previousPolygon2PointIndex;
            if (!poly1Point.IsAlmostTheSameAs(points2[poly2PointIndex]))
                return false;
            polygon2Index = poly2PointIndex;
        }

        return true;
    }
}