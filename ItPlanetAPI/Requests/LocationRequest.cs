using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Requests;

public class LocationRequest
{
    [Required] [Range(-90.0, 90.0)] public double Latitude { get; set; }

    [Required] [Range(-180.0, 180.0)] public double Longitude { get; set; }
}