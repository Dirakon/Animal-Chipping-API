namespace ItPlanetAPI.Models;

public class AnimalUpdateRequest
{
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChippingLocationId { get; set; }
    public string LifeStatus { get; set; }

    public bool IsValid()
    {
        if (Weight <= 0 || Length <= 0 || Height <= 0 || Gender is not "MALE" and not "FEMALE" and not "OTHER" ||
            ChipperId <= 0 || ChippingLocationId <= 0 || LifeStatus is not "ALIVE" and not "DEAD")
            return false;
        return true;
    }
}