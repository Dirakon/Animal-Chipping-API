using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Dtos;

public class LocationDto
{
    [Required] public double Latitude { get; set; }
    [Required] public double Longitude { get; set; }
    [Required] public long Id { get; set; }
}