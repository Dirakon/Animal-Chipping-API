using ItPlanetAPI.Middleware.ValidationAttributes;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Requests;

public class AnimalSearchRequest
{
    public DateTimeOffset StartDateTime { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset EndDateTime { get; set; } = DateTimeOffset.MaxValue;

    [Positive(true)] public int? ChipperId { get; set; }

    [Positive(true)] public long? ChippingLocationId { get; set; }

    public AnimalLifeStatus? LifeStatus { get; set; }

    public AnimalGender? Gender { get; set; }

    [NonNegative] public int From { get; set; } = 0;

    [Positive] public int Size { get; set; } = 10;
}