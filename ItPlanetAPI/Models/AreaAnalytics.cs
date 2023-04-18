using ItPlanetAPI.Dtos;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Controllers;

internal class AreaAnalytics
{
    private readonly Area _area;
    private readonly DateTimeOffset _endDate;
    private readonly DateTimeOffset _startDate;
    public readonly Dictionary<long, AnimalTypeAnalyticsDto> TypeIdToTypeAnalytics = new();
    public long TotalAnimalsArrived, TotalQuantityAnimals, TotalAnimalsGone;

    public AreaAnalytics(Area area, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        _area = area;
        _startDate = startDate;
        _endDate = endDate;
    }

    public void AnalyzeAnimalMovements(Animal animal)
    {
        var visitTimesAndLocations = animal.VisitedLocations
            .Select(locationRelationship =>
                (locationRelationship.DateTimeOfVisitLocationPoint, locationRelationship.Location))
            .Prepend((DateTimeOffset.MinValue, animal.ChippingLocation))
            .TakeWhile(visitTimeAndLocation => visitTimeAndLocation.Item1 <= _endDate);

        bool arrivedToArea = false, previouslyInsideArea = false, leftArea = false;
        foreach (var (visitTime, location) in visitTimesAndLocations)
            (arrivedToArea, previouslyInsideArea, leftArea) = (
                    currentlyInsideArea: _area.ContainsOrOnBoundary(location),
                    arrivedInsideTimeFrame: visitTime >= _startDate) switch
                {
                    (currentlyInsideArea: true, arrivedInsideTimeFrame: true) when !previouslyInsideArea =>
                    (
                        arrivedToArea: true,
                        previouslyInsideArea: true,
                        leftArea
                    ),
                    (currentlyInsideArea: false, arrivedInsideTimeFrame: true) when previouslyInsideArea =>
                    (
                        arrivedToArea,
                        previouslyInsideArea: false,
                        leftArea: true
                    ),
                    (var currentlyInsideArea, arrivedInsideTimeFrame: true) =>
                    (
                        arrivedToArea,
                        previouslyInsideArea: currentlyInsideArea,
                        leftArea
                    ),
                    (var currentlyInsideArea, arrivedInsideTimeFrame: false) =>
                    (
                        arrivedToArea,
                        previouslyInsideArea: currentlyInsideArea,
                        leftArea
                    )
                };

        var typeIdsAndNames =
            animal.AnimalTypes.Select(typeRelationship => (typeRelationship.TypeId, typeRelationship.Type.Type));
        foreach (var (typeId, typeName) in typeIdsAndNames)
        {
            var typeAnalytics = GetTypeAnalytics(typeId, typeName);
            if (previouslyInsideArea) typeAnalytics.QuantityAnimals++;

            if (arrivedToArea) typeAnalytics.AnimalsArrived++;

            if (leftArea) typeAnalytics.AnimalsGone++;
        }

        if (previouslyInsideArea) TotalQuantityAnimals++;

        if (arrivedToArea) TotalAnimalsArrived++;

        if (leftArea) TotalAnimalsGone++;
    }

    private AnimalTypeAnalyticsDto GetTypeAnalytics(long typeId, string typeName)
    {
        if (TypeIdToTypeAnalytics.TryGetValue(typeId, out var analytics)) return analytics;

        var newAnalytics = new AnimalTypeAnalyticsDto
            {AnimalType = typeName, AnimalTypeId = typeId, AnimalsGone = 0, AnimalsArrived = 0, QuantityAnimals = 0};
        TypeIdToTypeAnalytics[typeId] = newAnalytics;
        return newAnalytics;
    }
}