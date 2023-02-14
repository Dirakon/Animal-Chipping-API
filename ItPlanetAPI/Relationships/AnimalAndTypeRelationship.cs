namespace ItPlanetAPI.Models;

public class AnimalAndTypeRelationship
{
    public long Id { get; set; }
    public long AnimalId { get; set; }
    public virtual Animal Animal { get; set; }
    public long TypeId { get; set; }
    public virtual AnimalType Type { get; set; }
    

    public void Remove(DatabaseContext databaseContext)
    {
        databaseContext.Remove(this);
    }

    public void ChangeTypeTo(long newTypeId)
    {
        TypeId = newTypeId;
    }

    public void InitializeRelationship()
    { 
        Animal.AnimalTypes.Add(this);
        Type.Animals.Add(this);
    }
}