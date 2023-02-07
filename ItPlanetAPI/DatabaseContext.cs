using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI;

public class DatabaseContext : DbContext
{
    public DatabaseContext (DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
}