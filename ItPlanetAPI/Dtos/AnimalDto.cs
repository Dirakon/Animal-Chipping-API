namespace ItPlanetAPI.Models;

public class AnimalDto
{
    public List<long> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChippingLocationId { get; set; }


    public long Id { get; set; }
    public string LifeStatus { get; set; }
    public DateTimeOffset ChippingDateTime { get; set; }
    public List<long> VisitedLocations { get; set; }
    public DateTimeOffset? DeathDateTime { get; set; }
}