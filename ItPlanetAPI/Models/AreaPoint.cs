namespace ItPlanetAPI.Models;

public class AreaPoint : ISpatial
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public long Id { get; set; }
    public virtual Area Area { get; set; }
}