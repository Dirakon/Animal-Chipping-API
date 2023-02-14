namespace ItPlanetAPI.Requests;

public class AnimalLocationSearchRequest
{
    public DateTime StartDateTime { get; set; } = DateTime.MinValue;
    public DateTime EndDateTime { get; set; } = DateTime.MaxValue;
    public int From { get; set; } = 0;
    public int Size { get; set; } = 10;
}