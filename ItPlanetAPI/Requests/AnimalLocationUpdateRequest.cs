namespace ItPlanetAPI.Requests;

public class AnimalLocationUpdateRequest
{
    public long VisitedLocationPointId { get; set; }
    public long LocationPointId { get; set; }

    public bool IsValid() => LocationPointId > 0 && VisitedLocationPointId > 0;
}