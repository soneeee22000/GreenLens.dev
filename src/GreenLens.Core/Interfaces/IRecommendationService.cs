using GreenLens.Core.Models;
using GreenLens.Shared.DTOs;

namespace GreenLens.Core.Interfaces;

/// <summary>
/// Interface for generating AI-powered carbon reduction recommendations.
/// </summary>
public interface IRecommendationService
{
    /// <summary>
    /// Generates reduction recommendations for a completed carbon estimate.
    /// </summary>
    Task<List<RecommendationResponse>> GenerateAsync(
        CarbonEstimate estimate,
        CancellationToken cancellationToken = default);
}
