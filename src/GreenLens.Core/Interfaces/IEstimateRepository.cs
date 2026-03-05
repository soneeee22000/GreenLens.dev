using GreenLens.Core.Models;

namespace GreenLens.Core.Interfaces;

/// <summary>
/// Repository interface for persisting and retrieving carbon estimates.
/// </summary>
public interface IEstimateRepository
{
    /// <summary>
    /// Saves a new carbon estimate to the database.
    /// </summary>
    Task<CarbonEstimate> CreateAsync(CarbonEstimate estimate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single estimate by ID, including resource breakdown.
    /// </summary>
    Task<CarbonEstimate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all estimates, ordered by creation date descending.
    /// </summary>
    Task<List<CarbonEstimate>> ListAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of estimates.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
