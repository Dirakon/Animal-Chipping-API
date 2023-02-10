namespace ItPlanetAPI.Models;

public class AnimalAndTypeRelationship
{
    public long AnimalId { get; set; }
    public virtual Animal Animal { get; set; }
    public long TypeId { get; set; }
    public virtual AnimalType Type { get; set; }
}