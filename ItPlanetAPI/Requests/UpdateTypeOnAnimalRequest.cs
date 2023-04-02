using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalTypeUpdateRequest
{
    [Required] [Positive] public long OldTypeId { get; set; }

    [Required] [Positive] public long NewTypeId { get; set; }
}