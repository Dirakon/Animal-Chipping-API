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
        return Weight > 0 && Length > 0 && Height > 0 && Gender is "MALE" or "FEMALE" or "OTHER" &&
               ChipperId > 0 && ChippingLocationId > 0 && LifeStatus is "ALIVE" or "DEAD";
    }
}