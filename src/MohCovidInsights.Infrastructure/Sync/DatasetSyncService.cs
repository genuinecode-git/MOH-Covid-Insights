using System.Security.Cryptography;
using System.Text;

using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Infrastructure.DataGovSg;
using MohCovidInsights.Infrastructure.Persistence;

namespace MohCovidInsights.Infrastructure.Sync;

public sealed class DatasetSyncService(AppDbContext db, IDataGovSgClient client, IServiceProvider services, 
    IOptions<DataGovSgOptions> options, IClock clock, ILogger<DatasetSyncService> logger) : IDatasetSyncService
{
    private readonly DataGovSgOptions _options = options.Value;

    public async Task<SyncSummary> SyncAllAsync(CancellationToken ct = default)
    {
        int succeeded = 0, skipped = 0, failed = 0, rows = 0;
        var first = true;

        foreach (var dataset in _options.Datasets)
        {
            if (!first) await Task.Delay(_options.DatasetDelay, ct);
            first = false;

            var run = DatasetSyncRun.Start(dataset.Id, clock.UtcNow);
            db.SyncRuns.Add(run);

            try
            {
                var payload = await client.DownloadAsync(dataset.Id, ct);
                var hash = ComputeHash(payload.Raw);

                if (await IsUnchangedAsync(dataset.Id, hash, ct))
                {
                    run.Skip(hash, clock.UtcNow);
                    skipped++;
                    logger.LogInformation("Dataset {DatasetId} unchanged, skipping", dataset.Id);
                }
                else
                {
                    var parser = services.GetRequiredKeyedService<IDatasetParser>(dataset.Parser);
                    var observations = parser.Parse(payload.Table);

                    await UpsertAsync(observations, ct);

                    run.Succeed(observations.Count, hash, clock.UtcNow);
                    succeeded++;
                    rows += observations.Count;
                    logger.LogInformation("Dataset {DatasetId} ingested {Rows} observations",
                        dataset.Id, observations.Count);
                }
            }
            catch (Exception ex)
            {
                run.Fail(ex.Message, clock.UtcNow);
                failed++;
                logger.LogError(ex, "Dataset {DatasetId} sync failed", dataset.Id);
            }
            
            await db.SaveChangesAsync(ct);
        }

        return new SyncSummary(succeeded, skipped, failed, rows);
    }

    private async Task<bool> IsUnchangedAsync(string datasetId, string hash, CancellationToken ct)
    {
        var runs = await db.SyncRuns
            .Where(r => r.DatasetId == datasetId && r.Status == SyncStatus.Succeeded)
            .Select(r => new { r.StartedAt, r.PayloadHash })
            .ToListAsync(ct);

        var latest = runs.OrderByDescending(r => r.StartedAt).FirstOrDefault();
        return latest?.PayloadHash == hash;
    }

    private async Task UpsertAsync(IReadOnlyList<WeeklyObservation> observations, CancellationToken ct)
    {
        if (observations.Count == 0) return;

        var metrics = observations.Select(o => o.MetricCode).Distinct().ToList();

        var existing = await db.Observations
            .Where(o => metrics.Contains(o.MetricCode))
            .ToDictionaryAsync(o => (o.EpiWeekKey, o.MetricCode, o.Dimension), ct);

        foreach (var incoming in observations)
        {
            var key = (incoming.EpiWeekKey, incoming.MetricCode, incoming.Dimension);
            if (existing.TryGetValue(key, out var current))
                db.Entry(current).CurrentValues.SetValues(incoming);
            else
                db.Observations.Add(incoming);
        }
    }

    private static string ComputeHash(string content) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
}
