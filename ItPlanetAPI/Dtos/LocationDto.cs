using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Dtos;

public class LocationDto : ISpatial
{
    [Required] public long Id { get; set; }
    [Required] public double Latitude { get; set; }
    [Required] public double Longitude { get; set; }
}