using System.Linq.Expressions;
using g3;

namespace ItPlanetAPI.Models;

public interface ISpatial
{
    public double Longitude { get; }
    public double Latitude { get; }
}

public static class ISpatialExtensions
{
    public static bool IsAlmostTheSameAs(this ISpatial first, ISpatial second)
    {
        return IsAlmostTheSameAs(second).Compile().Invoke(first);
    }

    public static Vector2d AsVector(this ISpatial spatial)
    {
        return new Vector2d(spatial.Latitude, spatial.Longitude);
    }


    public static Expression<Func<ISpatial, bool>> IsAlmostTheSameAs(ISpatial other)
    {
        const double epsilon = 0.0001;
        return current => other.Latitude - current.Latitude < epsilon &&
                          -other.Latitude + current.Latitude < epsilon &&
                          other.Longitude - current.Longitude < epsilon &&
                          -other.Longitude + current.Longitude < epsilon;
    }
}