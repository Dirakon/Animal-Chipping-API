namespace ItPlanetAPI.Dtos;

public class AnimalLocationDto
{
    public required  long Id { get; set; }
    public required  long LocationPointId { get; set; }
    public required   DateTimeOffset DateTimeOfVisitLocationPoint { get; set; }
}