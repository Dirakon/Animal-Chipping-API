namespace ItPlanetAPI.Models;

public class AnimalType
{
    public string Type { get; set; }
    public long Id { get; set; }

}
public class AnimalTypeRequest
{
    public string Type { get; set; }

    public bool IsValid() => !string.IsNullOrEmpty(Type);
}