namespace ItPlanetAPI.Models;

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool IsValid()
    {
        return Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;
    }
}