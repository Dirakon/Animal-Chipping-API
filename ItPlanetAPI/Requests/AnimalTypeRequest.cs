using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalTypeRequest
{
    [Required] [NonWhitespace] public string Type { get; set; }
}