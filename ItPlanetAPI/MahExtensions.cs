namespace ItPlanetAPI;

public static class MahExtensions
{
    
    public static bool AlmostEqualTo(this double value1, double value2)
    {
        return Math.Abs(value1 - value2) < 0.0000001; 
    }
    public static bool AlmostEqualTo(this float value1, float value2)
    {
        return Math.Abs(value1 - value2) < 0.0000001; 
    }
    public static bool AlmostEqualTo(this float value1, double value2)
    {
        return Math.Abs(value1 - value2) < 0.0000001; 
    }
    public static bool AlmostEqualTo(this double value1, float value2)
    {
        return Math.Abs(value1 - value2) < 0.0000001; 
    }
}