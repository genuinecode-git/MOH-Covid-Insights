using MohCovidInsights.Domain.Entities;

namespace MohCovidInsights.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WeeklyObservation> Observations => Set<WeeklyObservation>();
    public DbSet<DatasetSyncRun> SyncRuns => Set<DatasetSyncRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}