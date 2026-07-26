using Microsoft.EntityFrameworkCore;
using MohCovidInsights.Domain.Entities;

namespace MohCovidInsights.Infrastructure.Persistence;

public abstract class BaseAppDbContext : DbContext
{
    protected BaseAppDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<WeeklyObservation> Observations => Set<WeeklyObservation>();
    public DbSet<DatasetSyncRun> SyncRuns => Set<DatasetSyncRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseAppDbContext).Assembly);
}

public sealed class AppDbContext : BaseAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}

public sealed class ReadOnlyDbContext : BaseAppDbContext
{
    public ReadOnlyDbContext(DbContextOptions<ReadOnlyDbContext> options)
        : base(options)
    {
    }
}
