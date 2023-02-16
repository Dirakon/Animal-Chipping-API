using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalTypeUpdateRequest
{
    [Positive] public required  long OldTypeId { get; set; }

    [Positive] public required  long NewTypeId { get; set; }
}