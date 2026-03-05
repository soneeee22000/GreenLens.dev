using GreenLens.Core.Interfaces;
using GreenLens.Core.Models;
using GreenLens.Core.Services;
using GreenLens.Shared.DTOs;
using Moq;

namespace GreenLens.Core.Tests;

/// <summary>
/// Unit tests for the core carbon estimation logic.
/// </summary>
public class CarbonEstimationServiceTests
{
    private readonly Mock<IEmissionFactorService> _mockEmissionFactorService;
    private readonly Mock<IEstimateRepository> _mockEstimateRepository;
    private readonly CarbonEstimationService _service;

    public CarbonEstimationServiceTests()
    {
        _mockEmissionFactorService = new Mock<IEmissionFactorService>();
        _mockEstimateRepository = new Mock<IEstimateRepository>();

        _mockEstimateRepository
            .Setup(r => r.CreateAsync(It.IsAny<CarbonEstimate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CarbonEstimate e, CancellationToken _) => e);

        _service = new CarbonEstimationService(
            _mockEmissionFactorService.Object,
            _mockEstimateRepository.Object);
    }

    [Fact]
    public async Task EstimateAsync_WithValidComputeResource_CalculatesCorrectCo2e()
    {
        // Arrange
        var factor = new EmissionFactor
        {
            ResourceType = "Standard_D4s_v3",
            Region = "westeurope",
            Co2ePerUnit = 0.035m,
            Unit = "kgCO2e/hour",
            Source = "test"
        };

        _mockEmissionFactorService
            .Setup(s => s.GetFactorAsync("Standard_D4s_v3", "westeurope", It.IsAny<CancellationToken>()))
            .ReturnsAsync(factor);

        var request = new EstimateRequest
        {
            Resources = new List<ResourceUsageRequest>
            {
                new()
                {
                    ResourceType = "Standard_D4s_v3",
                    Quantity = 2,
                    Hours = 720,
                    Region = "westeurope"
                }
            }
        };

        // Act
        var result = await _service.EstimateAsync(request);

        // Assert: 0.035 * 2 * 720 = 50.4
        Assert.Equal(50.4m, result.TotalCo2eKg);
        Assert.Single(result.Resources);
        Assert.Equal(50.4m, result.Resources[0].Co2eKg);
    }

    [Fact]
    public async Task EstimateAsync_WithStorageResource_CalculatesWithoutHours()
    {
        // Arrange
        var factor = new EmissionFactor
        {
            ResourceType = "BlobStorage",
            Region = "eastus",
            Co2ePerUnit = 0.001m,
            Unit = "kgCO2e/GB/month",
            Source = "test"
        };

        _mockEmissionFactorService
            .Setup(s => s.GetFactorAsync("BlobStorage", "eastus", It.IsAny<CancellationToken>()))
            .ReturnsAsync(factor);

        var request = new EstimateRequest
        {
            Resources = new List<ResourceUsageRequest>
            {
                new()
                {
                    ResourceType = "BlobStorage",
                    Quantity = 500,
                    Hours = 0,
                    Region = "eastus"
                }
            }
        };

        // Act
        var result = await _service.EstimateAsync(request);

        // Assert: 0.001 * 500 = 0.5 (hours not used for storage)
        Assert.Equal(0.5m, result.TotalCo2eKg);
    }

    [Fact]
    public async Task EstimateAsync_WithMultipleResources_SumsTotal()
    {
        // Arrange
        _mockEmissionFactorService
            .Setup(s => s.GetFactorAsync("Standard_D4s_v3", "westeurope", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionFactor
            {
                ResourceType = "Standard_D4s_v3",
                Region = "westeurope",
                Co2ePerUnit = 0.035m,
                Unit = "kgCO2e/hour",
                Source = "test"
            });

        _mockEmissionFactorService
            .Setup(s => s.GetFactorAsync("BlobStorage", "westeurope", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionFactor
            {
                ResourceType = "BlobStorage",
                Region = "westeurope",
                Co2ePerUnit = 0.001m,
                Unit = "kgCO2e/GB/month",
                Source = "test"
            });

        var request = new EstimateRequest
        {
            Resources = new List<ResourceUsageRequest>
            {
                new() { ResourceType = "Standard_D4s_v3", Quantity = 1, Hours = 100, Region = "westeurope" },
                new() { ResourceType = "BlobStorage", Quantity = 200, Hours = 0, Region = "westeurope" }
            }
        };

        // Act
        var result = await _service.EstimateAsync(request);

        // Assert: (0.035 * 1 * 100) + (0.001 * 200) = 3.5 + 0.2 = 3.7
        Assert.Equal(3.7m, result.TotalCo2eKg);
        Assert.Equal(2, result.Resources.Count);
    }

    [Fact]
    public async Task EstimateAsync_WithUnsupportedResourceType_ThrowsAppException()
    {
        // Arrange
        _mockEmissionFactorService
            .Setup(s => s.GetFactorAsync("UnknownVM", "eastus", It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmissionFactor?)null);

        var request = new EstimateRequest
        {
            Resources = new List<ResourceUsageRequest>
            {
                new() { ResourceType = "UnknownVM", Quantity = 1, Hours = 100, Region = "eastus" }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _service.EstimateAsync(request));
        Assert.Contains("UnknownVM", ex.Message);
        Assert.Equal("UNSUPPORTED_RESOURCE_TYPE", ex.Code);
    }

    [Theory]
    [InlineData("kgCO2e/hour", 0.035, 2, 720, 50.4)]
    [InlineData("kgCO2e/GB/month", 0.001, 500, 0, 0.5)]
    [InlineData("kgCO2e/unit/month", 0.5, 3, 0, 1.5)]
    public void CalculateCo2e_ReturnsCorrectValue(
        string unit, decimal co2ePerUnit, int quantity, decimal hours, decimal expected)
    {
        var factor = new EmissionFactor
        {
            Co2ePerUnit = co2ePerUnit,
            Unit = unit
        };

        var result = CarbonEstimationService.CalculateCo2e(factor, quantity, hours);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task EstimateAsync_PersistsToRepository()
    {
        // Arrange
        _mockEmissionFactorService
            .Setup(s => s.GetFactorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionFactor
            {
                Co2ePerUnit = 0.01m,
                Unit = "kgCO2e/hour",
                Source = "test"
            });

        var request = new EstimateRequest
        {
            Resources = new List<ResourceUsageRequest>
            {
                new() { ResourceType = "Standard_B1s", Quantity = 1, Hours = 1, Region = "eastus" }
            }
        };

        // Act
        await _service.EstimateAsync(request);

        // Assert
        _mockEstimateRepository.Verify(
            r => r.CreateAsync(It.IsAny<CarbonEstimate>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
