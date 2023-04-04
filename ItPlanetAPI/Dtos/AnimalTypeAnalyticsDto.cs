namespace ItPlanetAPI.Dtos;

public class AnimalTypeAnalyticsDto
{
    public string AnimalType { get; set; }
    public long AnimalTypeId { get; set; }
    public long QuantityAnimals { get; set; }
    public long AnimalsGone { get; set; }
    public long AnimalsArrived { get; set; }
}