namespace ItPlanetAPI.Models;

public class AnimalAndLocationRelationship
{
    public long Id { get; set; }
    public long AnimalId { get; set; }
    public virtual Animal Animal { get; set; }
    public long LocationPointId { get; set; }
    public virtual Location Location { get; set; }
    public DateTime DataTimeOfVisitLocationPoint { get; set; } = DateTime.Now;

    public void ChangeLocationTo(Location newLocation)
    {
        Location.AnimalsVisitedHere.Remove(this);

        LocationPointId = newLocation.Id;
        Location = newLocation;
        Location.AnimalsVisitedHere.Add(this);
    }


    public void Remove(DatabaseContext databaseContext)
    {
        databaseContext.Remove(this);
        //
        // Location.AnimalsVisitedHere.Remove(this);
        // Animal.VisitedLocations.Remove(this);
        // Animal = null;
        // Location = null;
        // AnimalId = -1;
        // LocationPointId = -1;
    }

    public void InitializeRelationship()
    {
        Animal.VisitedLocations.Add(this);
        Location.AnimalsVisitedHere.Add(this);
    }
}