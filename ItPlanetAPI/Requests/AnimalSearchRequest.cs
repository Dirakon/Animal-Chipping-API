using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalSearchRequest
{
    public DateTimeOffset? StartDateTime { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset? EndDateTime { get; set; } = DateTimeOffset.MaxValue;

    [Positive(true)] public int? ChipperId { get; set; }

    [Positive(true)] public long? ChippingLocationId { get; set; }

    [AnimalLifeStatus(true)] public string? LifeStatus { get; set; }

    [AnimalGender(true)] public string? Gender { get; set; }

    [NonNegative] public int From { get; set; } = 0;

    [Positive] public int Size { get; set; } = 10;
}