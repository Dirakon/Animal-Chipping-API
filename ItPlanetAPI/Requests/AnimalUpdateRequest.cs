using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalUpdateRequest
{
    [Positive] public float Weight { get; set; }

    [Positive] public float Length { get; set; }

    [Positive] public float Height { get; set; }

    [AnimalGender] public string Gender { get; set; }

    [Positive] public int ChipperId { get; set; }

    [Positive] public long ChippingLocationId { get; set; }

    [AnimalLifeStatus] public string LifeStatus { get; set; }
}