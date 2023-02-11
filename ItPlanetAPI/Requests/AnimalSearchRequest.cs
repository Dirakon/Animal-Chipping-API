namespace ItPlanetAPI.Controllers;

public class AnimalSearchRequest
{
    public DateTime? StartDateTime { get; set; } = DateTime.MinValue;
    public DateTime? EndDateTime { get; set; } = DateTime.MaxValue;
    public int? ChipperId { get; set; }
    public long? ChippingLocationId { get; set; }
    public string? LifeStatus { get; set; }
    public string? Gender { get; set; }
    public int From { get; set; } = 0;
    public int Size { get; set; } = 10;
    
    
    public bool IsValid() =>
        From >= 0 && Size > 0 && ChipperId is not <= 0 &&
        ChippingLocationId is not <= 0 &&
        LifeStatus is "ALIVE" or "DEAD" or null &&
        Gender is "MALE" or "FEMALE" or "OTHER" or null;
}