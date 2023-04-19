using NetTopologySuite.Mathematics;

namespace ItPlanetAPI.Extensions.Geometry;

public static class VectorExtensions
{
    public static bool EpsilonEquals(this Vector2D vector1, Vector2D vector2,
        double epsilon = MathExtensions.StandardMathEpsilon)
    {
        return vector1.Distance(vector2) <= epsilon;
    }
}