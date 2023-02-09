namespace ItPlanetAPI.Models;


public class Animal
{
    // Settable in request body
    public long[] AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChipperLocationId { get; set; }


    // Non-settable in request body
    public int Id { get; set; }
    public string LifeStatus { get; set; } = "ALIVE";
    public DateTime ChippingDateTime { get; set; } = DateTime.Now;
    public long[] VisitedLocations { get; set; } = Array.Empty<long>();
    public DateTime? DeathDateTime { get; set; } = null;

}

public class AnimalRequest
{
    // Settable in request body
    public long[] AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChipperLocationId { get; set; }
}