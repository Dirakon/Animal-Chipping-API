namespace ItPlanetAPI.Models;

public class AnimalTypeRequest
{
    public string Type { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Type);
    }
}