namespace ItPlanetAPI.Models;

public class AnimalType
{
    public string Type { get; set; }
    public long Id { get; set; }
    public virtual ICollection<AnimalAndTypeRelationship> Animals { get; set; }

    public AnimalType()
    {
        Animals = new List<AnimalAndTypeRelationship>();
    }
}