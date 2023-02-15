using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalLocationUpdateRequest
{
    [Positive] public long VisitedLocationPointId { get; set; }

    [Positive] public long LocationPointId { get; set; }
}