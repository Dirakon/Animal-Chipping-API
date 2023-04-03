using AutoMapper;
using ItPlanetAPI.Requests;

namespace ItPlanetAPI.Models;

public class AreaPoint : ISpatial
{
    public long Id { get; set; }
    public virtual Area Area { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public static async Task<AreaPoint> CreateFrom(AreaPointCreationRequest areaPointRequest, IMapper mapper,
        DatabaseContext databaseContext, Area area)
    {
        var areaPoint = mapper.Map<AreaPoint>(areaPointRequest);
        areaPoint.Area = area;
        return areaPoint;
    }
}