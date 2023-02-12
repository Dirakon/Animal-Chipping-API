namespace ItPlanetAPI.Models;

public class AnimalType
{
    public AnimalType()
    {
        Animals = new List<AnimalAndTypeRelationship>();
    }

    public string Type { get; set; }
    public long Id { get; set; }
    public virtual ICollection<AnimalAndTypeRelationship> Animals { get; set; }
}