namespace ItPlanetAPI.Models;

public class AnimalAndTypeRelationship
{
    public long AnimalId { get; set; }
    public virtual Animal Animal { get; set; }
    public long TypeId { get; set; }
    public virtual AnimalType Type { get; set; }
    

    public void Remove(DatabaseContext databaseContext)
    {
        databaseContext.Remove(this);
        // Type.Animals.Remove(this);
        // Animal.AnimalTypes.Remove(this);
        // Animal = null;
        // Type = null;
        // AnimalId = -1;
        // TypeId = -1;
    }

    public void ChangeTypeTo(AnimalType newType)
    {
        Type.Animals.Remove(this);

        TypeId = newType.Id;
        Type = newType;
    }

    public void InitializeRelationship()
    { 
        Animal.AnimalTypes.Add(this);
        Type.Animals.Add(this);
    }
}