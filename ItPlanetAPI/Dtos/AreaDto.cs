namespace ItPlanetAPI.Dtos;

public class AreaDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<AreaPointDto> AreaPoints { get; set; }
}