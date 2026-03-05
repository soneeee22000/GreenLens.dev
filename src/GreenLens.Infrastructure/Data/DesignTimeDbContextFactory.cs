using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GreenLens.Infrastructure.Data;

/// <summary>
/// Factory for EF Core design-time operations (migrations).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GreenLensDbContext>
{
    public GreenLensDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GreenLensDbContext>();
        optionsBuilder.UseSqlite("Data Source=greenlens.db");
        return new GreenLensDbContext(optionsBuilder.Options);
    }
}
