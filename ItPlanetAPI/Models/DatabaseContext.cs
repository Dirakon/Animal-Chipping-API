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