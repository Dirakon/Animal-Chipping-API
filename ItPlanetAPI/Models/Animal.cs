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


    public int Id { get; set; }

    // TODO: optimize into enum
    public string LifeStatus { get; set; } = "ALIVE";
    public DateTime ChippingDateTime { get; set; } = DateTime.Now;
    public virtual ICollection<AnimalAndLocation> VisitedLocations { get; set; }
    public DateTime? DeathDateTime { get; set; } = null;

    public static async Task<Either<int, Animal>> From(AnimalRequest request, IMapper mapper,
        DatabaseContext databaseContext)
    {
        var animal = mapper.Map<Animal>(request);
        animal.LifeStatus = "ALIVE";


        return (await GetNeededEntities(databaseContext, request))
            .Some(
                a =>
                {
                    var (chippingAccount, chippingLocation, typesToConnectTo) = a;
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
        GetNeededEntities(DatabaseContext databaseContext, AnimalRequest request)
    {
        // TODO: change logic if the type order is important
        var typesToConnectTo =
            await databaseContext.AnimalTypes.Where(animalType => request.AnimalTypes.Contains(animalType.Id))
                .ToListAsync();

        var chippingLocation =
            databaseContext.AnimalLocations.Find(location => location.Id == request.ChippingLocationId);

        return chippingLocation.Match(
            chippingLocation =>
            {
                var chippingAccount = databaseContext.Accounts.Find(account => account.Id == request.ChipperId);

                return chippingAccount.Match(
                    chippingAccount => Option<(Account, AnimalLocation, List<AnimalType>)>.Some((chippingAccount,
                        chippingLocation, typesToConnectTo)),
                    Option<(Account, AnimalLocation, List<AnimalType>)>.None
                );
            },
            Option<(Account, AnimalLocation, List<AnimalType>)>.None
        );
    }
}

public class AnimalRequest
{
    public List<long> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChippingLocationId { get; set; }

    // Should only be used if comes from PUT request.
    public string LifeStatus { get; set; }

    public bool IsValid()
    {
        if (!AnimalTypes.Any())
            return false;
        if (AnimalTypes.Any(animalType => animalType <= 0))
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

public class AnimalDto
{
    public List<long> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }
    public int ChipperId { get; set; }
    public long ChipperLocationId { get; set; }


    public int Id { get; set; }
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