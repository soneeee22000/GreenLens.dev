using GreenLens.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenLens.Infrastructure.Data;

/// <summary>
/// EF Core database context for GreenLens.
/// </summary>
public class GreenLensDbContext : DbContext
{
    public GreenLensDbContext(DbContextOptions<GreenLensDbContext> options)
        : base(options)
    {
    }

    public DbSet<CarbonEstimate> CarbonEstimates => Set<CarbonEstimate>();
    public DbSet<ResourceUsage> ResourceUsages => Set<ResourceUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarbonEstimate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalCo2eKg).HasPrecision(18, 6);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasMany(e => e.Resources)
                .WithOne()
                .HasForeignKey(r => r.CarbonEstimateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<ResourceUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Co2eKg).HasPrecision(18, 6);
            entity.Property(e => e.Co2ePerUnit).HasPrecision(18, 8);
            entity.Property(e => e.Hours).HasPrecision(10, 2);
            entity.Property(e => e.ResourceType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Region).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(30).IsRequired();
        });
    }
}
