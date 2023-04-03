using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Dtos;

public class AreaPointDto : ISpatial
{
    [Required] public double Latitude { get; set; }
    [Required] public double Longitude { get; set; }
}