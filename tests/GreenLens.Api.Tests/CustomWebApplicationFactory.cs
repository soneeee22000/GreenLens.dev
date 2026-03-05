using GreenLens.Core.Interfaces;
using GreenLens.Core.Models;
using GreenLens.Infrastructure.Data;
using GreenLens.Shared.DTOs;
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

            // Register mock IEmissionFactorService with configured test data
            var mockEmissionFactorService = new Mock<IEmissionFactorService>();
            mockEmissionFactorService
                .Setup(s => s.GetFactorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string resourceType, string region, CancellationToken _) => new EmissionFactor
                {
                    Id = Guid.NewGuid().ToString(),
                    ResourceType = resourceType,
                    Region = region,
                    Provider = "Azure",
                    Co2ePerUnit = 0.035m,
                    Unit = "kgCO2e/hour",
                    Source = "test",
                    EffectiveDate = DateTime.UtcNow
                });
            services.AddScoped(_ => mockEmissionFactorService.Object);

            // Register mock IRecommendationService
            var mockRecommendationService = new Mock<IRecommendationService>();
            mockRecommendationService
                .Setup(s => s.GenerateAsync(It.IsAny<CarbonEstimate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RecommendationResponse>
                {
                    new() { Title = "Use B-series VMs", Description = "Burstable VMs reduce idle energy consumption.", EstimatedReductionPercent = 30, Effort = "Low" },
                    new() { Title = "Move to greener regions", Description = "Sweden Central has lower grid carbon intensity.", EstimatedReductionPercent = 25, Effort = "Medium" },
                    new() { Title = "Right-size resources", Description = "Downsize over-provisioned VMs based on utilization data.", EstimatedReductionPercent = 20, Effort = "Medium" }
                });
            services.AddScoped(_ => mockRecommendationService.Object);

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
