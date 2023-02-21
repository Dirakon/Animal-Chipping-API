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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        InitAnimalAndLocationRelationship(modelBuilder);
        InitAnimalAndTypeRelationship(modelBuilder);
        InitAnimalAndAccountRelationship(modelBuilder);

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONNECTION_STRING")))
            return;

        // TODO: remove (currently, test accounts for test purposes)
        modelBuilder.Entity<Account>().HasData(
            new Account {Id = 1, Email = "a@w.p", Password = "0", FirstName = "AWP", LastName = "Cool"},
            new Account {Id = 2, Email = "a@w.s", Password = "0", FirstName = "AWS", LastName = "Rich"}
        );
        modelBuilder.Entity<AnimalType>().HasData(
            new AnimalType {Id = 1, Type = "Doggie"},
            new AnimalType {Id = 2, Type = "Kitty"}
        );
        modelBuilder.Entity<Location>().HasData(
            new Location {Id = 1, Longitude = 69, Latitude = 69},
            new Location {Id = 2, Longitude = -69, Latitude = -69}
        );
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