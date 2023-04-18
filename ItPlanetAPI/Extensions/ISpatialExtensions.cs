using System.Linq.Expressions;
using System.Text;
using ItPlanetAPI.Extensions;
using NetTopologySuite.Geometries;

namespace ItPlanetAPI.Models;

public static class ISpatialExtensions
{
    public static bool IsAlmostTheSameAs(this ISpatial first, ISpatial second)
    {
        return IsAlmostTheSameAs(second).Compile().Invoke(first);
    }

    public static Coordinate AsCoordinate(this ISpatial spatial)
    {
        return new Coordinate(spatial.Latitude, spatial.Longitude);
    }


    public static Expression<Func<ISpatial, bool>> IsAlmostTheSameAs(ISpatial other)
    {
        const double epsilon = GeometryExtensions.StandardGeometryEpsilon;
        return current => other.Latitude - current.Latitude < epsilon &&
                          -other.Latitude + current.Latitude < epsilon &&
                          other.Longitude - current.Longitude < epsilon &&
                          -other.Longitude + current.Longitude < epsilon;
    }

    /**
     * Get standard GeoHash of some coordinates
     * (the implementation is based on https://github.com/Postlagerkarte/geohash-dotnet/blob/master/src/Geohasher.cs)
     */
    public static string HashedV1(this ISpatial spatial)
    {
        var base32Chars = "0123456789bcdefghjkmnpqrstuvwxyz".ToCharArray();
        int[] bits = {16, 8, 4, 2, 1};
        const int precision = 12;

        var geohash = new StringBuilder();
        double[] latInterval = {-90.0, 90.0};
        double[] lonInterval = {-180.0, 180.0};
        var isEven = true;
        var bit = 0;
        var ch = 0;

        var longitude = spatial.Longitude;
        var latitude = spatial.Latitude;
        while (geohash.Length < precision)
        {
            double mid;

            if (isEven)
            {
                mid = (lonInterval[0] + lonInterval[1]) / 2;

                if (longitude > mid)
                {
                    ch |= bits[bit];
                    lonInterval[0] = mid;
                }
                else
                {
                    lonInterval[1] = mid;
                }
            }
            else
            {
                mid = (latInterval[0] + latInterval[1]) / 2;

                if (latitude > mid)
                {
                    ch |= bits[bit];
                    latInterval[0] = mid;
                }
                else
                {
                    latInterval[1] = mid;
                }
            }

            isEven = !isEven;

            if (bit < 4)
            {
                bit++;
            }
            else
            {
                geohash.Append(base32Chars[ch]);
                bit = 0;
                ch = 0;
            }
        }

        return geohash.ToString();
    }

    /**
     * Get the standard GeoHash of coordinates and further encode it into Base64
     */
    public static string HashedV2(this ISpatial spatial)
    {
        var hashV1 = spatial.HashedV1();
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(hashV1));
    }

    public static string HashedV3(this ISpatial spatial)
    {
        return spatial.HashedV2();
    }
}