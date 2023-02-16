using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalUpdateRequest
{
    [Positive] public required  float Weight { get; set; }

    [Positive] public required  float Length { get; set; }

    [Positive] public required  float Height { get; set; }

    [AnimalGender] public required  string Gender { get; set; }

    [Positive] public required  int ChipperId { get; set; }

    [Positive] public required  long ChippingLocationId { get; set; }

    [AnimalLifeStatus] public required  string LifeStatus { get; set; }
}