namespace ItPlanetAPI.Models;

public class AnimalType
{
    public string Type { get; set; }
    public long Id { get; set; }
    public virtual ICollection<AnimalAndType> Animals { get; set; }

    public AnimalType()
    {
        Animals = new List<AnimalAndType>();
    }
}

public class AnimalTypeRequest
{
    public string Type { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Type);
    }
}

public class AnimalTypeDto
{
    public string Type { get; set; }
    public long Id { get; set; }
}