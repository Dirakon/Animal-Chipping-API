using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalLocationUpdateRequest
{
    [Positive] public required  long VisitedLocationPointId { get; set; }

    [Positive] public required  long LocationPointId { get; set; }
}