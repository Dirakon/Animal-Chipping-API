using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalTypeRequest
{
    [NonWhitespace] public string Type { get; set; }
}