using Microsoft.EntityFrameworkCore;
using PostgreIntegrationTestExample.Models;

namespace PostgreIntegrationTestExample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}
