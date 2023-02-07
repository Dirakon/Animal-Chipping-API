using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI;

public class MyDbContext : DbContext
{
    public MyDbContext (DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
}