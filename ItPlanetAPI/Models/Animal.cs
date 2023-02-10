using System.Diagnostics;
using AutoMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Models;

public class Animal
{
    public virtual ICollection<AnimalAndType> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }

    public float Height { get; set; }

    // TODO: optimize into enum
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public virtual Account Chipper { get; set; }
    public long ChippingLocationId { get; set; }
    public virtual AnimalLocation ChippingLocation { get; set; }


    public long Id { get; set; }

    // TODO: optimize into enum
    public string LifeStatus { get; set; } = "ALIVE";
    public DateTime ChippingDateTime { get; set; } = DateTime.Now;
    public virtual ICollection<AnimalAndLocation> VisitedLocations { get; set; }
    public DateTime? DeathDateTime { get; set; } = null;

    public Animal()
    {
        VisitedLocations = new List<AnimalAndLocation>();
        AnimalTypes = new List<AnimalAndType>();
    }

    public static async Task<Either<int, Animal>> From(AnimalCreationRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {
        var animal = mapper.Map<Animal>(request);
        animal.LifeStatus = "ALIVE";


        return (await GetNeededEntities(databaseContext, animalTypes: request.AnimalTypes, chippingLocationId: request.ChippingLocationId, chipperId: request.ChipperId))
            .Some(
                neededEntities =>
                {
                    var (chippingAccount, chippingLocation, typesToConnectTo) = neededEntities;
                    chippingAccount.ChippedAnimals.Add(animal);
                    animal.Chipper = chippingAccount;

                    chippingLocation.AnimalsChippedHere.Add(animal);
                    animal.ChippingLocation = chippingLocation;

                    foreach (var animalType in typesToConnectTo)
                    {
                        var newRelationship = new AnimalAndType {Animal = animal, Type = animalType};
                        animalType.Animals.Add(newRelationship);
                        animal.AnimalTypes.Add(newRelationship);
                    }

                    return Either<int, Animal>.Right(animal);
                }
            )
            .None(Either<int, Animal>.Left(404));
    }

    private static async Task<Option<(Account chippingAccount, AnimalLocation chippingLocation, List<AnimalType>
            typesToConnectTo)>>
        GetNeededEntities(DatabaseContext databaseContext, List<long>? animalTypes, long chippingLocationId, long chipperId)
    {
        // TODO: change logic if the type order is important
        var typesToConnectTo = animalTypes == null? new List<AnimalType>() :
            await databaseContext.AnimalTypes.Where(animalType => animalTypes.Contains(animalType.Id))
                .ToListAsync();
        
        if (animalTypes != null && animalTypes.Count != typesToConnectTo.Count)
            return Option<(Account, AnimalLocation, List<AnimalType>)>.None;

        var chippingLocation =
            databaseContext.AnimalLocations.Find(location => location.Id == chippingLocationId);

        return chippingLocation.Match(
            chippingLocation =>
            {
                var chippingAccount = databaseContext.Accounts.Find(account => account.Id == chipperId);

                return chippingAccount.Match(
                    chippingAccount => Option<(Account, AnimalLocation, List<AnimalType>)>.Some((chippingAccount,
                        chippingLocation, typesToConnectTo)),
                    Option<(Account, AnimalLocation, List<AnimalType>)>.None
                );
            },
            Option<(Account, AnimalLocation, List<AnimalType>)>.None
        );
    }

    public async Task<Either<int, Unit>> TryTakeValuesOf(AnimalUpdateRequest request, IMapper mapper, DatabaseContext databaseContext)
    {
        
        if (LifeStatus ==  "DEAD" && request.LifeStatus == "ALIVE")
            return Either<int, Unit>.Left(400);
        if (VisitedLocations.Any() && VisitedLocations.First().LocationId == request.ChippingLocationId)
            return Either<int, Unit>.Left(400);
        
        return (await GetNeededEntities(databaseContext, animalTypes: null, chipperId: request.ChipperId, chippingLocationId: request.ChippingLocationId))
            .Some(
                neededEntities =>
                {
                    var (chippingAccount, chippingLocation, _) = neededEntities;
                    
                    
                    if (this.Chipper != chippingAccount)
                    {
                        this.Chipper.ChippedAnimals.Remove(this);
                        chippingAccount.ChippedAnimals.Add(this);
                        this.Chipper = chippingAccount;
                    }

                    if (this.ChippingLocation != chippingLocation)
                    {
                        this.ChippingLocation.AnimalsChippedHere.Remove(this);
                        chippingLocation.AnimalsChippedHere.Add(this);
                        this.ChippingLocation = chippingLocation;
                    }
                    return Either<int, Unit>.Right(Unit.Default);
                }
            )
            .None(Either<int, Unit>.Left(404));
    }
}

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

        if (Weight <= 0 || Length <= 0 || Height <= 0 || Gender is not "MALE" and not "FEMALE" and not "OTHER" ||
            ChipperId <= 0 || ChippingLocationId <= 0)
            return false;
        return true;
    }

    public bool HasConflicts()
    {
        return AnimalTypes.GroupBy(x => x).Any(g => g.Count() > 1);
    }
}
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

        if (Weight <= 0 || Length <= 0 || Height <= 0 || Gender is not "MALE" and not "FEMALE" and not "OTHER" ||
            ChipperId <= 0 || ChippingLocationId <= 0 || LifeStatus is not "ALIVE" and not "DEAD")
            return false;
        return true;
    }

}

public class AnimalDto
{
    public List<long> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChippingLocationId { get; set; }


    public long Id { get; set; }
    public string LifeStatus { get; set; }
    public DateTime ChippingDateTime { get; set; }
    public List<long> VisitedLocations { get; set; }
    public DateTime? DeathDateTime { get; set; }
}

public class AnimalAndLocation
{
    public long AnimalId { get; set; }
    public virtual Animal Animal { get; set; }
    public long LocationId { get; set; }
    public virtual AnimalLocation Location { get; set; }
}

public class AnimalAndType
{
    public long AnimalId { get; set; }
    public virtual Animal Animal { get; set; }
    public long TypeId { get; set; }
    public virtual AnimalType Type { get; set; }
}