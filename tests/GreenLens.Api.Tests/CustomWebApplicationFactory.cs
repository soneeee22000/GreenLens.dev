using GreenLens.Core.Interfaces;
using GreenLens.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GreenLens.Api.Tests;

/// <summary>
/// Custom factory that replaces real DB with in-memory SQLite for integration tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<GreenLensDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Remove any hosted services that call Migrate
            var hostedServiceDescriptors = services
                .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService))
                .ToList();

            // Add test DbContext with shared connection
            services.AddDbContext<GreenLensDbContext>(options =>
                options.UseSqlite(_connection));

            // Register mock IEmissionFactorService (not implemented until Phase 2)
            services.AddScoped(_ => new Mock<IEmissionFactorService>().Object);

            // Create the schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GreenLensDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
