using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Domain.ValueObjects;
using MohCovidInsights.Infrastructure.Persistence;

namespace MohCovidInsights.Infrastructure.Repositories;

public sealed class ObservationRepository(AppDbContext db) : IObservationRepository
{
    public async Task<IReadOnlyList<WeeklyObservation>> GetAllDimensionsAsync(EpiWeekRange range, IReadOnlyCollection<MetricCode> metrics, CancellationToken ct = default)
    {
        var from = range.From.ToKey();
        var to = range.To.ToKey();
        var codes = metrics.Select(m => m.Value).ToList();

        return await db.Observations
            .AsNoTracking()
            .Where(o => o.EpiWeekKey >= from
                        && o.EpiWeekKey <= to
                        && codes.Contains(o.MetricCode))
            .OrderBy(o => o.EpiWeekKey)
            .ThenBy(o => o.MetricCode)
            .ThenBy(o => o.Dimension)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WeeklyObservation>> GetAsync(EpiWeekRange range, IReadOnlyCollection<MetricCode> metrics, string dimension, CancellationToken ct = default)
    {
        var from = range.From.ToKey();
        var to = range.To.ToKey();
        var codes = metrics.Select(m => m.Value).ToList();

        return await db.Observations
            .AsNoTracking()
            .Where(o => o.EpiWeekKey >= from
                        && o.EpiWeekKey <= to
                        && o.Dimension == dimension
                        && codes.Contains(o.MetricCode))
            .OrderBy(o => o.EpiWeekKey)
            .ToListAsync(ct);
    }

    public async Task<EpiWeek?> GetLatestWeekAsync(CancellationToken ct = default)
    {
        var key = await db.Observations.AsNoTracking()
            .OrderByDescending(o => o.EpiWeekKey)
            .Select(o => (int?)o.EpiWeekKey)
            .FirstOrDefaultAsync(ct);

        return key is null ? null : EpiWeek.FromKey(key.Value);
    }
}
