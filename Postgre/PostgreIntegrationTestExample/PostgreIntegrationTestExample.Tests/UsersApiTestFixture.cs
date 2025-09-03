using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PostgreIntegrationTestExample.Data;
using PostgreIntegrationTestExample.Models;

public class UsersApiTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer DbContainer { get; private set; }
    public WebApplicationFactory<Program>? Factory { get; private set; }
    public HttpClient? Client { get; private set; }

    public UsersApiTestFixture()
    {
        DbContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(DbContainer.GetConnectionString()));
                });
            });

        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    public async Task DisposeAsync()
    {
        if (DbContainer != null)
            await DbContainer.DisposeAsync();
        Client?.Dispose();
        Factory?.Dispose();
    }

    public async Task SeedUsersAsync(params User[] users)
    {
        using var scope = Factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.RemoveRange(db.Users); // Clear table
        await db.SaveChangesAsync();
        db.Users.AddRange(users);
        await db.SaveChangesAsync();
    }
}
