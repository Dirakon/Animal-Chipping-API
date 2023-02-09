namespace ItPlanetAPI.Models;

public class AnimalLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public long Id { get; set; }
    public virtual ICollection<AnimalAndLocation> AnimalsVisitedHere { get; set; }
    public virtual ICollection<Animal> AnimalsChippedHere { get; set; }
}

public class AnimalLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool IsValid()
    {
        return Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;
    }
}

public class AnimalLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public long Id { get; set; }
}