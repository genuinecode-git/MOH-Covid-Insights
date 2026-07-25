namespace MohCovidInsights.Infrastructure.Sync.Interfaces;

public interface IDatasetSyncService
{
    Task<SyncSummary> SyncAllAsync(CancellationToken ct = default);
}
