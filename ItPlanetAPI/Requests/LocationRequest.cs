using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Requests;

public class LocationRequest
{
    [Range(-90.0, 90.0)] public required  double Latitude { get; set; }

    [Range(-180.0, 180.0)] public required  double Longitude { get; set; }
}