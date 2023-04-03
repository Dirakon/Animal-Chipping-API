using System.Linq.Expressions;
using ItPlanetAPI.Relationships;
using ItPlanetAPI.Requests;

namespace ItPlanetAPI.Models;

public class Location : ISpatial
{
    public Location()
    {
        AnimalsChippedHere = new List<Animal>();
        AnimalsVisitedHere = new List<AnimalAndLocationRelationship>();
    }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public long Id { get; set; }
    public virtual ICollection<AnimalAndLocationRelationship> AnimalsVisitedHere { get; set; }
    public virtual ICollection<Animal> AnimalsChippedHere { get; set; }

}