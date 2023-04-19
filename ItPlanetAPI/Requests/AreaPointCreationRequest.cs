using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Requests;

public class AreaPointCreationRequest : ISpatial
{
    [Required] [Range(-180.0, 180.0)] public double Longitude { get; set; }

    [Required] [Range(-90.0, 90.0)] public double Latitude { get; set; }
}