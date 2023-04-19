namespace ItPlanetAPI.Extensions;

public static class MathExtensions
{
    public const double StandardMathEpsilon = 0.0001;

    public static bool AlmostEqualTo(this double value1, double value2)
    {
        return Math.Abs(value1 - value2) < StandardMathEpsilon;
    }

    public static bool AlmostEqualTo(this float value1, float value2)
    {
        return Math.Abs(value1 - value2) < StandardMathEpsilon;
    }

    public static bool AlmostEqualTo(this float value1, double value2)
    {
        return Math.Abs(value1 - value2) < StandardMathEpsilon;
    }

    public static bool AlmostEqualTo(this double value1, float value2)
    {
        return Math.Abs(value1 - value2) < StandardMathEpsilon;
    }
}