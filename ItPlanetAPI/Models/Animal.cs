using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Models;

public class Animal
{
    public Animal()
    {
        VisitedLocations = new List<AnimalAndLocationRelationship>();
        AnimalTypes = new List<AnimalAndTypeRelationship>();
    }

    public virtual ICollection<AnimalAndTypeRelationship> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }

    public float Height { get; set; }

    // TODO: optimize into enum
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public virtual Account Chipper { get; set; }
    public long ChippingLocationId { get; set; }
    public virtual Location ChippingLocation { get; set; }


    public long Id { get; set; }

    // TODO: optimize into enum
    public string LifeStatus { get; set; } = "ALIVE";
    public DateTime ChippingDateTime { get; set; } = DateTime.Now;
    public virtual ICollection<AnimalAndLocationRelationship> VisitedLocations { get; set; }
    public DateTime? DeathDateTime { get; set; } = null;

    public static async Task<Animal?> TryCreateFrom(AnimalCreationRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {


        return (await GetNeededEntities(databaseContext, request.AnimalTypes, request.ChippingLocationId,
                request.ChipperId)) switch
            {
                var (chippingAccount, chippingLocation, typesToConnectTo) => CreateFrom(request, mapper,
                    chippingAccount, chippingLocation, typesToConnectTo),
                null => null
            };
    }

    private static Animal CreateFrom(AnimalCreationRequest request, IMapper mapper, Account chippingAccount, Location chippingLocation, List<AnimalType> typesToConnectTo)
    {
        var animal = mapper.Map<Animal>(request);
        animal.LifeStatus = "ALIVE";
        
        chippingAccount.ChippedAnimals.Add(animal);
        animal.Chipper = chippingAccount;

        chippingLocation.AnimalsChippedHere.Add(animal);
        animal.ChippingLocation = chippingLocation;

        foreach (var animalType in typesToConnectTo)
        {
            var newRelationship = new AnimalAndTypeRelationship {Animal = animal, Type = animalType};
            animalType.Animals.Add(newRelationship);
            animal.AnimalTypes.Add(newRelationship);
        }

        return animal;

    }
    private static async Task<(Account chippingAccount, Location chippingLocation, List<AnimalType>
            typesToConnectTo)?>
        GetNeededEntities(DatabaseContext databaseContext, List<long>? animalTypes, long chippingLocationId,
            long chipperId)
    {
        // TODO: change logic if the type order is important
        var typesToConnectTo = animalTypes == null
            ? new List<AnimalType>()
            : await databaseContext.AnimalTypes.Where(animalType => animalTypes.Contains(animalType.Id))
                .ToListAsync();

        if (animalTypes != null && animalTypes.Count != typesToConnectTo.Count)
            return null;

        var locationAndAccount = (
            await databaseContext.Locations.FirstOrDefaultAsync(location => location.Id == chippingLocationId),
            await databaseContext.Accounts.FirstOrDefaultAsync(account => account.Id == chipperId)
        );


        return locationAndAccount switch{
            ({} location, {} account) => (account,location,typesToConnectTo),
            _ => null
        };
    }

    public bool IsRequestAppropriate(AnimalUpdateRequest request)
    {
        if (LifeStatus == "DEAD" && request.LifeStatus == "ALIVE")
            return false;
        if (VisitedLocations.Any() && VisitedLocations.First().LocationPointId == request.ChippingLocationId)
            return false;
        return true;
    }
    
    public async Task<bool> TryTakeValuesOf(AnimalUpdateRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {
        var neededEntities = await GetNeededEntities(databaseContext, null, chipperId: request.ChipperId,
            chippingLocationId: request.ChippingLocationId);
        
        if (neededEntities is ({ } newChippingAccount,{} newChippingLocation, _))
        {
            if (Chipper != newChippingAccount)
            {
                Chipper.ChippedAnimals.Remove(this);
                newChippingAccount.ChippedAnimals.Add(this);
                Chipper = newChippingAccount;
            }
                
            if (ChippingLocation != newChippingLocation)
            {
                ChippingLocation.AnimalsChippedHere.Remove(this);
                newChippingLocation.AnimalsChippedHere.Add(this);
                ChippingLocation = newChippingLocation;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}