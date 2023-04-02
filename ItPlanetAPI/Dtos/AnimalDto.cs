using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Dtos;

public class AnimalDto
{
    [Required] public List<long> AnimalTypes { get; set; }
    [Required] public float Weight { get; set; }
    [Required] public float Length { get; set; }
    [Required] public float Height { get; set; }
    [Required] public string Gender { get; set; }
    [Required] public int ChipperId { get; set; }
    [Required] public long ChippingLocationId { get; set; }


    [Required] public long Id { get; set; }
    [Required] public string LifeStatus { get; set; }
    [Required] public DateTimeOffset ChippingDateTime { get; set; }
    [Required] public List<long> VisitedLocations { get; set; }
    [Required] public DateTimeOffset? DeathDateTime { get; set; }
}