using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalTypeRequest
{
    [NonWhitespace] public required  string Type { get; set; }
}