using AutoMapper;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Relationships;
using ItPlanetAPI.Requests;
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

    public AnimalGender Gender { get; set; }
    public int ChipperId { get; set; }
    public virtual Account Chipper { get; set; }
    public long ChippingLocationId { get; set; }
    public virtual Location ChippingLocation { get; set; }


    public long Id { get; set; }

    public AnimalLifeStatus LifeStatus { get; set; } = AnimalLifeStatus.Alive;
    public DateTimeOffset ChippingDateTime { get; set; } = DateTimeOffset.Now.AsWholeMilliseconds();
    public virtual ICollection<AnimalAndLocationRelationship> VisitedLocations { get; set; }
    public DateTimeOffset? DeathDateTime { get; set; }

    public static async Task<Animal?> TryCreateFrom(AnimalCreationRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {
        return await GetNeededEntities(databaseContext, request.AnimalTypes, request.ChippingLocationId,
                request.ChipperId) switch
            {
                var (chippingAccount, chippingLocation, typesToConnectTo) => await CreateFrom(request, mapper,
                    chippingAccount, chippingLocation, typesToConnectTo, databaseContext),
                null => null
            };
    }

    private static async Task<Animal> CreateFrom(AnimalCreationRequest request, IMapper mapper, Account chippingAccount,
        Location chippingLocation, List<AnimalType> typesToConnectTo, DatabaseContext databaseContext)
    {
        var animal = mapper.Map<Animal>(request);
        animal.LifeStatus = AnimalLifeStatus.Alive;
        databaseContext.Animals.Add(animal);

        animal.Chipper = chippingAccount;

        animal.ChippingLocation = chippingLocation;

        foreach (var newRelationship in typesToConnectTo
                     .Select(animalType => new AnimalAndTypeRelationship
                     {
                         Animal = animal, AnimalId = animal.Id, Type = animalType, TypeId = animalType.Id
                     })
                )
            newRelationship.InitializeRelationship();

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
        if (LifeStatus == AnimalLifeStatus.Dead && request.LifeStatus == AnimalLifeStatus.Alive)
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

        if (neededEntities is not ({ } newChippingAccount, { } newChippingLocation, _)) return false;

        if (request.LifeStatus == AnimalLifeStatus.Dead && LifeStatus == AnimalLifeStatus.Alive)
            DeathDateTime = DateTimeOffset.Now.AsWholeMilliseconds();

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

        mapper.Map(request, this);
        return true;
    }
}