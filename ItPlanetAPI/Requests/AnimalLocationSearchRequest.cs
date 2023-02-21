using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalLocationSearchRequest
{
    public DateTimeOffset StartDateTime { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset EndDateTime { get; set; } = DateTimeOffset.MaxValue;
    [NonNegative] public int From { get; set; } = 0;
    [Positive] public int Size { get; set; } = 10;
}