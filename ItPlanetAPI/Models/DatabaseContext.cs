using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Models;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        // TODO: remove (currently, test accounts for test purposes)
        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1,  Email = "a@w.p", Password = "0", FirstName = "AWP", LastName = "Cool"},
            new Account { Id = 2, Email = "a@w.s", Password = "0", FirstName = "AWS", LastName = "Rich"}
        );
        modelBuilder.Entity<AnimalType>().HasData(
            new AnimalType(){Id = 1, Type = "Doggie"},
            new AnimalType(){Id = 2, Type = "Kitty"}
        );
        modelBuilder.Entity<Animal>().HasData(
            new Animal()
                {AnimalTypes = new long[]{1,2}, ChipperId = 1, ChipperLocationId = 1, Gender = "Male", Height = 169,Length = 2, Weight = 2, Id = 1}
        );
        modelBuilder.Entity<AnimalLocation>().HasData(
            new AnimalLocation(){Id = 1,Longitude = 69, Latitude = 69},
            new AnimalLocation(){Id = 2,Longitude = -69, Latitude = -69}
        );
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AnimalType> AnimalTypes { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<AnimalLocation> AnimalLocations { get; set; }
}