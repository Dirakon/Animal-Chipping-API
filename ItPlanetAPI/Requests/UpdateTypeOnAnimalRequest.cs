using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalTypeUpdateRequest
{
    [Positive] public long OldTypeId { get; set; }

    [Positive] public long NewTypeId { get; set; }
}