using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Dtos;

public class AnimalTypeDto
{
    [Required] public string Type { get; set; }
    [Required] public long Id { get; set; }
}