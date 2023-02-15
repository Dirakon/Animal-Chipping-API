namespace ItPlanetAPI.Models;

public class AnimalCreationRequest
{
    public List<long> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChippingLocationId { get; set; }

    public bool IsValid()
    {
        if (!AnimalTypes.Any() || AnimalTypes.Any(animalType => animalType <= 0))
            return false;

        return Weight > 0 && Length > 0 && Height > 0 && Gender is "MALE" or "FEMALE" or "OTHER" &&
               ChipperId > 0 && ChippingLocationId > 0;
    }

    public bool HasConflicts()
    {
        return AnimalTypes.GroupBy(x => x).Any(g => g.Count() > 1);
    }
}