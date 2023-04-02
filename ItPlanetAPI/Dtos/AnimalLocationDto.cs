using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Dtos;

public class AnimalLocationDto
{
    [Required] public long Id { get; set; }
    [Required] public long LocationPointId { get; set; }
    [Required] public DateTimeOffset DateTimeOfVisitLocationPoint { get; set; }
}