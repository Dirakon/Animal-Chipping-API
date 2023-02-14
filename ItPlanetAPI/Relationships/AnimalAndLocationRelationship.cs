namespace ItPlanetAPI.Models;

public class AnimalAndLocationRelationship
{
    public long Id { get; set; }
    public long AnimalId { get; set; }
    public virtual Animal Animal { get; set; }
    public long LocationPointId { get; set; }
    public virtual Location Location { get; set; }
    public DateTimeOffset DateTimeOfVisitLocationPoint { get; set; } = DateTimeOffset.Now;

    public void ChangeLocationTo(long newLocationId)
    {
        LocationPointId = newLocationId;
    }


    public void Remove(DatabaseContext databaseContext)
    {
        databaseContext.Remove(this);
    }

    public void InitializeRelationship()
    {
        Animal.VisitedLocations.Add(this);
        Location.AnimalsVisitedHere.Add(this);
    }
}