namespace ItPlanetAPI.Dtos;

public class AnimalDto
{
    public required  List<long> AnimalTypes { get; set; }
    public required  float Weight { get; set; }
    public required  float Length { get; set; }
    public required  float Height { get; set; }
    public required  string Gender { get; set; }
    public required  int ChipperId { get; set; }
    public required  long ChippingLocationId { get; set; }


    public required  long Id { get; set; }
    public required  string LifeStatus { get; set; }
    public required  DateTimeOffset ChippingDateTime { get; set; }
    public required  List<long> VisitedLocations { get; set; }
    public required  DateTimeOffset? DeathDateTime { get; set; }
}