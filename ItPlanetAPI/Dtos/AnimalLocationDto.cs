namespace ItPlanetAPI.Models;

public class AnimalLocationDto
{
    public long Id { get; set; }
    public long LocationPointId { get; set; }
    public DateTimeOffset DateTimeOfVisitLocationPoint { get; set; }
}