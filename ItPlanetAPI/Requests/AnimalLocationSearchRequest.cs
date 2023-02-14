namespace ItPlanetAPI.Requests;

public class AnimalLocationSearchRequest
{
    public DateTimeOffset StartDateTime { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset EndDateTime { get; set; } = DateTimeOffset.MaxValue;
    public int From { get; set; } = 0;
    public int Size { get; set; } = 10;
}