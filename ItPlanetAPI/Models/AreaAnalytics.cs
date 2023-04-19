using ItPlanetAPI.Dtos;

namespace ItPlanetAPI.Models;

public record AreaAnalytics(Area Area, DateTimeOffset StartDate, DateTimeOffset EndDate)
{
    public readonly Dictionary<long, AnimalTypeAnalyticsDto> TypeIdToTypeAnalytics = new();
    public long TotalAnimalsArrived, TotalQuantityAnimals, TotalAnimalsGone;

    public void AnalyzeAnimalMovements(Animal animal)
    {
        var visitTimesAndLocations = animal.VisitedLocations
            .Select(locationRelationship =>
                (visitTime: locationRelationship.DateTimeOfVisitLocationPoint, location: locationRelationship.Location))
            .Prepend((visitTime: DateTimeOffset.MinValue, location: animal.ChippingLocation))
            .TakeWhile(visitTimeAndLocation => visitTimeAndLocation.visitTime <= EndDate);

        bool arrivedToArea = false, previouslyInsideArea = false, leftArea = false;
        foreach (var (visitTime, location) in visitTimesAndLocations)
            (arrivedToArea, previouslyInsideArea, leftArea) = (
                    currentlyInsideArea: Area.ContainsOrOnBoundary(location),
                    arrivedInsideTimeFrame: visitTime >= StartDate) switch
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