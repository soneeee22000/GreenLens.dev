using GreenLens.Core.Models;

namespace GreenLens.Core.Interfaces;

/// <summary>
/// Interface for querying emission factor data from Azure AI Search.
/// </summary>
public interface IEmissionFactorService
{
    /// <summary>
    /// Gets the emission factor for a specific resource type and region.
    /// Returns null if no matching factor is found.
    /// </summary>
    Task<EmissionFactor?> GetFactorAsync(string resourceType, string region, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches emission factors using natural language query via Azure AI Search.
    /// </summary>
    Task<List<EmissionFactor>> SearchAsync(string query, int top = 10, CancellationToken cancellationToken = default);
}
