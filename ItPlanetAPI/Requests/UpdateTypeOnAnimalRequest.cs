namespace ItPlanetAPI.Controllers;

public class AnimalTypeUpdateRequest
{
    public long OldTypeId { get; set; }
    public long NewTypeId { get; set; }

    public bool IsValid()
    {
        return OldTypeId > 0 && NewTypeId > 0;
    }
}