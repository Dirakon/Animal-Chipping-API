using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI;

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
    }

    public DbSet<Account> Accounts { get; set; }
}