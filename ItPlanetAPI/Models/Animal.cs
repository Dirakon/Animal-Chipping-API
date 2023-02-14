using AutoMapper;
using ItPlanetAPI.Extensions;
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
    public DateTimeOffset ChippingDateTime { get; set; } = DateTimeOffset.Now;
    public virtual ICollection<AnimalAndLocationRelationship> VisitedLocations { get; set; }
    public DateTimeOffset? DeathDateTime { get; set; } = null;

    public static async Task<Animal?> TryCreateFrom(AnimalCreationRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {
        return await GetNeededEntities(databaseContext, request.AnimalTypes, request.ChippingLocationId,
                request.ChipperId) switch
            {
                var (chippingAccount, chippingLocation, typesToConnectTo) => await CreateFrom(request, mapper,
                    chippingAccount, chippingLocation, typesToConnectTo,  databaseContext),
                null => null
            };
    }

    private static async Task<Animal> CreateFrom(AnimalCreationRequest request, IMapper mapper, Account chippingAccount,
        Location chippingLocation, List<AnimalType> typesToConnectTo, DatabaseContext databaseContext)
    {
        var animal = mapper.Map<Animal>(request);
        animal.LifeStatus = "ALIVE";
        databaseContext.Animals.Add(animal);
        
        animal.Chipper = chippingAccount;

        animal.ChippingLocation = chippingLocation;

        foreach (var animalType in typesToConnectTo)
        {
            var newRelationship = new AnimalAndTypeRelationship {Animal = animal, AnimalId = animal.Id, Type = animalType, TypeId = animalType.Id};
            newRelationship.InitializeRelationship();
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


        return locationAndAccount switch
        {
            ({ } location, { } account) => (account, location, typesToConnectTo),
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

        if (neededEntities is ({ } newChippingAccount, { } newChippingLocation, _))
        {
            if (request.LifeStatus == "DEAD" && LifeStatus == "ALIVE")
            {
                DeathDateTime = DateTimeOffset.Now;
            }
            
            if (ChipperId != newChippingAccount.Id)
            {
                Chipper = newChippingAccount;
                ChipperId = newChippingAccount.Id;
            }

            if (ChippingLocationId != newChippingLocation.Id)
            {
                ChippingLocation = newChippingLocation;
                ChippingLocationId = newChippingLocation.Id;
            }

            mapper.Map(request,this);
            return true;
        }

        return false;
    }
}