using System.Linq.Expressions;

namespace ItPlanetAPI.Models;

public class Location
{
    public Location()
    {
        AnimalsChippedHere = new List<Animal>();
        AnimalsVisitedHere = new List<AnimalAndLocationRelationship>();
    }
    public static Expression<Func<Location, bool>> IsAlmostTheSameAs(LocationRequest request)
    {
        const double epsilon = 0.0001;
        return current => (request.Latitude - current.Latitude) < epsilon && (-request.Latitude + current.Latitude) < epsilon && 
                          (request.Longitude - current.Longitude) < epsilon && (-request.Longitude + current.Longitude) < epsilon; 
    }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public long Id { get; set; }
    public virtual ICollection<AnimalAndLocationRelationship> AnimalsVisitedHere { get; set; }
    public virtual ICollection<Animal> AnimalsChippedHere { get; set; }
}