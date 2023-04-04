namespace ItPlanetAPI.Dtos;

public class AreaAnalyticsDto
{
    public List<AnimalTypeAnalyticsDto> AnimalsAnalytics { get; set; }
    public long TotalQuantityAnimals { get; set; }
    public long TotalAnimalsArrived { get; set; }
    public long TotalAnimalsGone { get; set; }
}