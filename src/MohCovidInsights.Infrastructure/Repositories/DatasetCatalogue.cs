using MohCovidInsights.Application.Abstractions;
using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Domain.ValueObjects;
using MohCovidInsights.Infrastructure.DataGovSg;
using MohCovidInsights.Infrastructure.Persistence;

namespace MohCovidInsights.Infrastructure.Repositories;

public sealed class DatasetCatalogue(AppDbContext db, IOptions<DataGovSgOptions> options) : IDatasetCatalogue
{
    public async Task<IReadOnlyList<DatasetCoverageInfo>> GetCoverageAsync(CancellationToken ct = default)
    {
        var completedRuns = await db.SyncRuns.AsNoTracking()
            .Where(r => r.Status == SyncStatus.Succeeded || r.Status == SyncStatus.Skipped)
            .Select(r => new { r.DatasetId, r.CompletedAt })
            .ToListAsync(ct);

        var lastSync = completedRuns
            .GroupBy(r => r.DatasetId)
            .ToDictionary(g => g.Key, g => g.Max(r => r.CompletedAt));

        var stats = await db.Observations.AsNoTracking()
            .GroupBy(o => o.MetricCode)
            .Select(g => new
            {
                MetricCode = g.Key,
                Min = g.Min(o => o.EpiWeekKey),
                Max = g.Max(o => o.EpiWeekKey),
                Weeks = g.Select(o => o.EpiWeekKey).Distinct().Count(),
            })
            .ToListAsync(ct);

        var byParser = MetricsByParser();

        return options.Value.Datasets.Select(d =>
        {
            var codes = byParser.GetValueOrDefault(d.Parser, []);
            var matching = stats.Where(s => codes.Contains(s.MetricCode)).ToList();

            return new DatasetCoverageInfo(
                d.Id,
                d.Name,
                matching.Count == 0 ? null : EpiWeek.FromKey(matching.Min(s => s.Min)),
                matching.Count == 0 ? null : EpiWeek.FromKey(matching.Max(s => s.Max)),
                matching.Count == 0 ? 0 : matching.Max(s => s.Weeks),
                lastSync.GetValueOrDefault(d.Id),
                d.Tone);
        }).ToList();
    }

    private static Dictionary<string, string[]> MetricsByParser() => new()
    {
        ["infections"] = [MetricCode.EstimatedInfections.Value],
        ["admissions"] = [MetricCode.HospitalAdmissions.Value, MetricCode.IcuAdmissions.Value],
        ["average-cases"] = [MetricCode.AvgHospitalisedCases.Value, MetricCode.AvgIcuCases.Value],
        ["icu-beds"] = [MetricCode.IcuBedsCovid.Value, MetricCode.IcuBedsNonCovid.Value, MetricCode.IcuBedsTotalCapacity.Value],
    };
}
