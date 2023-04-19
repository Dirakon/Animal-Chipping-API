using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace ItPlanetAPI.Extensions.Geometry;

public static class CoordinateExtensions
{
    public static Vector2D DirectionTo(this Coordinate coordinate1, Coordinate coordinate2)
    {
        return (new Vector2D(coordinate2) - new Vector2D(coordinate1)).Normalize();
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
            if (firstPoint.Equals2D(somePoint, MathExtensions.StandardMathEpsilon))
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
}