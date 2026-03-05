using GreenLens.Core.Models;
using GreenLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GreenLens.Infrastructure.Tests;

/// <summary>
/// Integration tests for EstimateRepository using in-memory SQLite.
/// </summary>
public class EstimateRepositoryTests : IDisposable
{
    private readonly GreenLensDbContext _context;
    private readonly EstimateRepository _repository;

    public EstimateRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GreenLensDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new GreenLensDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new EstimateRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_PersistsEstimateWithResources()
    {
        // Arrange
        var estimate = CreateTestEstimate();

        // Act
        var result = await _repository.CreateAsync(estimate);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        var saved = await _context.CarbonEstimates
            .Include(e => e.Resources)
            .FirstAsync(e => e.Id == estimate.Id);
        Assert.Equal(10.5m, saved.TotalCo2eKg);
        Assert.Single(saved.Resources);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEstimateWithResources()
    {
        // Arrange
        var estimate = CreateTestEstimate();
        await _repository.CreateAsync(estimate);

        // Act
        var result = await _repository.GetByIdAsync(estimate.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(estimate.Id, result.Id);
        Assert.Single(result.Resources);
        Assert.Equal("Standard_D4s_v3", result.Resources[0].ResourceType);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task ListAsync_ReturnsOrderedByCreatedAtDescending()
    {
        // Arrange
        var older = CreateTestEstimate();
        older.CreatedAt = DateTime.UtcNow.AddHours(-1);
        var newer = CreateTestEstimate();
        newer.CreatedAt = DateTime.UtcNow;

        await _repository.CreateAsync(older);
        await _repository.CreateAsync(newer);

        // Act
        var results = await _repository.ListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results[0].CreatedAt >= results[1].CreatedAt);
    }

    [Fact]
    public async Task ListAsync_RespectsPagination()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await _repository.CreateAsync(CreateTestEstimate());
        }

        // Act
        var page1 = await _repository.ListAsync(page: 1, pageSize: 2);
        var page2 = await _repository.ListAsync(page: 2, pageSize: 2);

        // Assert
        Assert.Equal(2, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _repository.CreateAsync(CreateTestEstimate());
        await _repository.CreateAsync(CreateTestEstimate());

        // Act
        var count = await _repository.CountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    private static CarbonEstimate CreateTestEstimate()
    {
        var id = Guid.NewGuid();
        return new CarbonEstimate
        {
            Id = id,
            TotalCo2eKg = 10.5m,
            CreatedAt = DateTime.UtcNow,
            Resources = new List<ResourceUsage>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CarbonEstimateId = id,
                    ResourceType = "Standard_D4s_v3",
                    Quantity = 2,
                    Hours = 720,
                    Region = "westeurope",
                    Co2eKg = 10.5m,
                    Co2ePerUnit = 0.035m,
                    Unit = "kgCO2e/hour"
                }
            }
        };
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
