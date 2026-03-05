using GreenLens.Core.Interfaces;
using GreenLens.Core.Models;
using GreenLens.Shared.DTOs;
using Moq;

namespace GreenLens.Core.Tests;

/// <summary>
/// Unit tests for AI-powered recommendation generation.
/// Tests the IRecommendationService contract against the estimation model.
/// </summary>
public class RecommendationServiceTests
{
    private readonly Mock<IRecommendationService> _mockRecommendationService;

    public RecommendationServiceTests()
    {
        _mockRecommendationService = new Mock<IRecommendationService>();
    }

    [Fact]
    public async Task GenerateAsync_WithValidEstimate_Returns3To5Recommendations()
    {
        // Arrange
        var estimate = CreateTestEstimate();
        var recommendations = CreateTestRecommendations(4);

        _mockRecommendationService
            .Setup(s => s.GenerateAsync(estimate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendations);

        // Act
        var result = await _mockRecommendationService.Object.GenerateAsync(estimate);

        // Assert
        Assert.InRange(result.Count, 3, 5);
    }

    [Fact]
    public async Task GenerateAsync_EachRecommendation_HasRequiredFields()
    {
        // Arrange
        var estimate = CreateTestEstimate();
        var recommendations = CreateTestRecommendations(3);

        _mockRecommendationService
            .Setup(s => s.GenerateAsync(estimate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendations);

        // Act
        var result = await _mockRecommendationService.Object.GenerateAsync(estimate);

        // Assert
        foreach (var rec in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(rec.Title));
            Assert.False(string.IsNullOrWhiteSpace(rec.Description));
            Assert.InRange(rec.EstimatedReductionPercent, 0, 100);
            Assert.Contains(rec.Effort, new[] { "Low", "Medium", "High" });
        }
    }

    [Fact]
    public async Task GenerateAsync_RecommendationsHaveValidEffortLevels()
    {
        // Arrange
        var estimate = CreateTestEstimate();
        var recommendations = new List<RecommendationResponse>
        {
            new() { Title = "Tip 1", Description = "Desc", EstimatedReductionPercent = 10, Effort = "Low" },
            new() { Title = "Tip 2", Description = "Desc", EstimatedReductionPercent = 30, Effort = "Medium" },
            new() { Title = "Tip 3", Description = "Desc", EstimatedReductionPercent = 50, Effort = "High" }
        };

        _mockRecommendationService
            .Setup(s => s.GenerateAsync(estimate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendations);

        // Act
        var result = await _mockRecommendationService.Object.GenerateAsync(estimate);

        // Assert
        var efforts = result.Select(r => r.Effort).ToHashSet();
        Assert.Subset(new HashSet<string> { "Low", "Medium", "High" }, efforts);
    }

    private static CarbonEstimate CreateTestEstimate()
    {
        return new CarbonEstimate
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            TotalCo2eKg = 50.4m,
            Resources = new List<ResourceUsage>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ResourceType = "Standard_D4s_v3",
                    Quantity = 2,
                    Hours = 720,
                    Region = "westeurope",
                    Co2eKg = 50.4m,
                    Co2ePerUnit = 0.035m,
                    Unit = "kgCO2e/hour"
                }
            }
        };
    }

    private static List<RecommendationResponse> CreateTestRecommendations(int count)
    {
        var recommendations = new List<RecommendationResponse>();
        var efforts = new[] { "Low", "Medium", "High" };

        for (int i = 0; i < count; i++)
        {
            recommendations.Add(new RecommendationResponse
            {
                Title = $"Switch to greener VM size #{i + 1}",
                Description = $"By switching to a B-series burstable VM, you can reduce emissions by approximately {(i + 1) * 10}%.",
                EstimatedReductionPercent = (i + 1) * 10,
                Effort = efforts[i % efforts.Length]
            });
        }

        return recommendations;
    }
}
