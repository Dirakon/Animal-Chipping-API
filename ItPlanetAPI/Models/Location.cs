using ItPlanetAPI.Relationships;

namespace ItPlanetAPI.Models;

public class Location : ISpatial
{
    public Location()
    {
        AnimalsChippedHere = new List<Animal>();
        AnimalsVisitedHere = new List<AnimalAndLocationRelationship>();
    }

    public long Id { get; set; }
    public virtual ICollection<AnimalAndLocationRelationship> AnimalsVisitedHere { get; set; }
    public virtual ICollection<Animal> AnimalsChippedHere { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}