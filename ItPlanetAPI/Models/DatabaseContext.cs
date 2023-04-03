using ItPlanetAPI.Relationships;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Models;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options)
        : base(options)
    {
    }


    public DbSet<Account> Accounts { get; set; }
    public DbSet<AnimalType> AnimalTypes { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Area> Areas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        InitAnimalAndLocationRelationship(modelBuilder);
        InitAnimalAndTypeRelationship(modelBuilder);
        InitAnimalAndAccountRelationship(modelBuilder);
        InitAreaAndAreaPointRelationship(modelBuilder);

        modelBuilder.Entity<Account>().HasData(
            new Account
            {
                Id = 1, Role = AccountRole.Admin, Email = "admin@simbirsoft.com", Password = "qwerty123",
                FirstName = "adminFirstName", LastName = "adminLastName"
            },
            new Account
            {
                Id = 2, Role = AccountRole.Chipper, Email = "chipper@simbirsoft.com", Password = "qwerty123",
                FirstName = "chipperFirstName", LastName = "chipperLastName"
            },
            new Account
            {
                Id = 3, Role = AccountRole.User, Email = "user@simbirsoft.com", Password = "qwerty123",
                FirstName = "userFirstName", LastName = "userLastName"
            }
        );
    }

    private static void InitAreaAndAreaPointRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AreaPoint>()
            .HasOne(a => a.Area)
            .WithMany(ao => ao.AreaPoints)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void InitAnimalAndLocationRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnimalAndLocationRelationship>()
            .HasOne(a => a.Location)
            .WithMany(ao => ao.AnimalsVisitedHere)
            .HasForeignKey(ao => ao.LocationPointId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AnimalAndLocationRelationship>()
            .HasOne(a => a.Animal)
            .WithMany(ao => ao.VisitedLocations)
            .HasForeignKey(ao => ao.AnimalId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Animal>()
            .HasOne(ao => ao.ChippingLocation)
            .WithMany(a => a.AnimalsChippedHere);
    }

    private static void InitAnimalAndTypeRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnimalAndTypeRelationship>()
            .HasOne(a => a.Type)
            .WithMany(ao => ao.Animals)
            .HasForeignKey(ao => ao.TypeId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AnimalAndTypeRelationship>()
            .HasOne(a => a.Animal)
            .WithMany(ao => ao.AnimalTypes)
            .HasForeignKey(ao => ao.AnimalId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void InitAnimalAndAccountRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>()
            .HasOne(ao => ao.Chipper)
            .WithMany(a => a.ChippedAnimals)
            .HasForeignKey(ao => ao.ChipperId);
    }
}