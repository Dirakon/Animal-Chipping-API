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
    public DbSet<AnimalLocation> AnimalLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        InitAnimalAndLocationRelationship(modelBuilder);
        InitAnimalAndTypeRelationship(modelBuilder);
        InitAnimalAndAccountRelationship(modelBuilder);

        // TODO: remove (currently, test accounts for test purposes)
        modelBuilder.Entity<Account>().HasData(
            new Account {Id = 1, Email = "a@w.p", Password = "0", FirstName = "AWP", LastName = "Cool"},
            new Account {Id = 2, Email = "a@w.s", Password = "0", FirstName = "AWS", LastName = "Rich"}
        );
        modelBuilder.Entity<AnimalType>().HasData(
            new AnimalType {Id = 1, Type = "Doggie"},
            new AnimalType {Id = 2, Type = "Kitty"}
        );
        // modelBuilder.Entity<Animal>().HasData(
        //     new Animal()
        //         {AnimalTypes = new List<long>{1,2}, ChipperId = 1, ChipperLocationId = 1, Gender = "Male", Height = 169,Length = 2, Weight = 2, Id = 1}
        // );
        modelBuilder.Entity<AnimalLocation>().HasData(
            new AnimalLocation {Id = 1, Longitude = 69, Latitude = 69},
            new AnimalLocation {Id = 2, Longitude = -69, Latitude = -69}
        );
    }

    private static void InitAnimalAndLocationRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnimalAndLocation>()
            .HasKey(ao => new {ao.AnimalId, ao.LocationId});

        modelBuilder.Entity<AnimalLocation>()
            .HasMany(ao => ao.AnimalsVisitedHere)
            .WithOne(a => a.Location)
            .HasForeignKey(ao => ao.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Animal>()
            .HasMany(ao => ao.VisitedLocations)
            .WithOne(a => a.Animal)
            .HasForeignKey(ao => ao.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<Animal>()
            .HasOne(ao => ao.ChippingLocation)
            .WithMany(a => a.AnimalsChippedHere)
            .HasForeignKey(ao => ao.ChippingLocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void InitAnimalAndTypeRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnimalAndType>()
            .HasKey(ao => new {ao.AnimalId, ao.TypeId});

        modelBuilder.Entity<AnimalType>()
            .HasMany(ao => ao.Animals)
            .WithOne(a => a.Type)
            .HasForeignKey(ao => ao.TypeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Animal>()
            .HasMany(ao => ao.AnimalTypes)
            .WithOne(a => a.Animal)
            .HasForeignKey(ao => ao.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void InitAnimalAndAccountRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>()
            .HasOne(ao => ao.Chipper)
            .WithMany(a => a.ChippedAnimals)
            .HasForeignKey(ao => ao.ChipperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}