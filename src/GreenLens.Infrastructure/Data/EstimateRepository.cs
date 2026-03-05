using GreenLens.Core.Interfaces;
using GreenLens.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenLens.Infrastructure.Data;

/// <summary>
/// EF Core implementation of the estimate repository.
/// </summary>
public class EstimateRepository : IEstimateRepository
{
    private readonly GreenLensDbContext _context;

    public EstimateRepository(GreenLensDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<CarbonEstimate> CreateAsync(
        CarbonEstimate estimate,
        CancellationToken cancellationToken = default)
    {
        _context.CarbonEstimates.Add(estimate);
        await _context.SaveChangesAsync(cancellationToken);
        return estimate;
    }

    /// <inheritdoc />
    public async Task<CarbonEstimate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.CarbonEstimates
            .Include(e => e.Resources)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<CarbonEstimate>> ListAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.CarbonEstimates
            .Include(e => e.Resources)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CarbonEstimates.CountAsync(cancellationToken);
    }
}
