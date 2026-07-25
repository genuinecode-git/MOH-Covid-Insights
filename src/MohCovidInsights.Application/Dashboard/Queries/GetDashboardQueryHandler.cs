using System.Globalization;

using MohCovidInsights.Application.Abstractions;
using MohCovidInsights.Application.Analytics;
using MohCovidInsights.Application.Dashboard.Dtos;
using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Application.Dashboard.Queries;

public sealed class GetDashboardQueryHandler(
    IObservationRepository observations,
    IDatasetCatalogue catalogue,
    IClock clock)
    : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    private static readonly MetricCode[] RequiredMetrics =
    [
        MetricCode.EstimatedInfections,
        MetricCode.HospitalAdmissions,
        MetricCode.IcuAdmissions,
        MetricCode.IcuBedsCovid,
        MetricCode.IcuBedsNonCovid,
        MetricCode.IcuBedsTotalCapacity,
    ];

    public async Task<Result<DashboardDto>> HandleAsync(GetDashboardQuery query, CancellationToken ct = default)
    {
        var rangeResult = EpiWeekRange.TryCreate(query.From, query.To);
        if (rangeResult.IsFailure) return rangeResult.Error;

        var range = rangeResult.Value;
        var dimension = NormaliseDimension(query.AgeGroup);

        var current = await observations.GetAsync(range, RequiredMetrics, dimension, ct);
        if (current.Count == 0)
            return Error.NotFound($"No observations available for {range}.");

        var previous = await observations.GetAsync(range.PreviousPeriod(), RequiredMetrics, dimension, ct);
        var coverage = await catalogue.GetCoverageAsync(ct);

        var series = BuildSeries(current, range);
        var previousSeries = BuildSeries(previous, range.PreviousPeriod());

        return new DashboardDto(
            Kpis: BuildKpis(series, previousSeries, range),
            Weekly: BuildWeekly(series, range),
            BedUtilisation: BuildBedUtilisation(series, range),
            Insights: BuildInsights(series),
            Coverage: BuildCoverage(coverage),
            LastUpdated: coverage.Max(c => c.LastSyncedAt)?.ToString("d MMM yyyy", CultureInfo.InvariantCulture)
                         ?? clock.UtcNow.ToString("d MMM yyyy", CultureInfo.InvariantCulture),
            RangeLabel: FormatRangeLabel(range));
    }

    private static string NormaliseDimension(string ageGroup) =>
        string.IsNullOrWhiteSpace(ageGroup) || ageGroup.Equals("All", StringComparison.OrdinalIgnoreCase)
            ? WeeklyObservation.Dimensions.All
            : ageGroup;

    private static Dictionary<string, List<SeriesPoint>> BuildSeries(
        IReadOnlyList<WeeklyObservation> rows, EpiWeekRange range)
    {
        var result = new Dictionary<string, List<SeriesPoint>>(StringComparer.Ordinal);

        foreach (var metric in RequiredMetrics)
            result[metric.Value] = [];

        foreach (var row in rows.Where(r => range.Contains(r.EpiWeek)))
            if (result.TryGetValue(row.MetricCode, out var list))
                list.Add(new SeriesPoint(row.EpiWeek, row.Value));

        foreach (var list in result.Values)
            list.Sort((a, b) => a.Week.CompareTo(b.Week));

        return result;
    }

    private static IReadOnlyList<SeriesPoint> Get(
        Dictionary<string, List<SeriesPoint>> series, MetricCode metric) => series[metric.Value];

    private IReadOnlyList<KpiDto> BuildKpis(
        Dictionary<string, List<SeriesPoint>> current,
        Dictionary<string, List<SeriesPoint>> previous,
        EpiWeekRange range)
    {
        var kpis = new List<KpiDto>(6)
        {
            TotalKpi("total-infections", "Total Estimated Infections", "infections",
            Get(current, MetricCode.EstimatedInfections), Get(previous, MetricCode.EstimatedInfections))
        };

        var peakInfections = SeriesAnalytics.Peak(Get(current, MetricCode.EstimatedInfections));
        kpis.Add(new KpiDto("peak-infection-week", "Peak Infection Week",
            peakInfections?.Week.ToCompactString() ?? "—", "peak",
            Caption: peakInfections is null ? null : $"Week of {FormatWeekSpan(peakInfections.Week)}"));

        kpis.Add(TotalKpi("total-hospital", "Total Hospital Admissions", "hospital",
            Get(current, MetricCode.HospitalAdmissions), Get(previous, MetricCode.HospitalAdmissions)));

        kpis.Add(TotalKpi("total-icu", "Total ICU Admissions", "icu",
            Get(current, MetricCode.IcuAdmissions), Get(previous, MetricCode.IcuAdmissions)));

        var utilisation = BuildUtilisationSeries(current);
        var peakUtilisation = SeriesAnalytics.Peak(utilisation);
        kpis.Add(new KpiDto("peak-icu-utilisation", "Peak ICU Utilisation",
            peakUtilisation is null ? "—" : $"{peakUtilisation.Value:0.0}%", "utilisation",
            Caption: peakUtilisation is null ? null : $"Week of {FormatWeekSpan(peakUtilisation.Week, withYear: true)}"));

        var latest = LatestWeek(current) ?? range.To;
        kpis.Add(new KpiDto("latest-week", "Latest Data Week",
            latest.ToCompactString(), "latest",
            Caption: $"Week of {FormatWeekSpan(latest, withYear: true)}"));

        return kpis;
    }

    private static KpiDto TotalKpi(
        string id, string label, string tone,
        IReadOnlyList<SeriesPoint> current, IReadOnlyList<SeriesPoint> previous)
    {
        var total = SeriesAnalytics.Total(current);
        var previousTotal = SeriesAnalytics.Total(previous);
        var change = SeriesAnalytics.PercentChange(previousTotal, total);

        return new KpiDto(id, label, FormatCount(total), tone,
            Trend: change is null
                ? null
                : new KpiTrendDto(Math.Abs(change.Value), change.Value >= 0 ? "up" : "down"));
    }

    private static List<SeriesPoint> BuildUtilisationSeries(Dictionary<string, List<SeriesPoint>> series)
    {
        var covid = Get(series, MetricCode.IcuBedsCovid).ToDictionary(p => p.Week, p => p.Value);
        var nonCovid = Get(series, MetricCode.IcuBedsNonCovid).ToDictionary(p => p.Week, p => p.Value);

        var result = new List<SeriesPoint>();
        foreach (var capacity in Get(series, MetricCode.IcuBedsTotalCapacity))
        {
            if (capacity.Value <= 0m) continue;
            covid.TryGetValue(capacity.Week, out var c);
            nonCovid.TryGetValue(capacity.Week, out var n);
            result.Add(new SeriesPoint(capacity.Week, Math.Round((c + n) / capacity.Value * 100m, 1)));
        }
        return result;
    }

    private static IReadOnlyList<WeeklyPointDto> BuildWeekly(
        Dictionary<string, List<SeriesPoint>> series, EpiWeekRange range)
    {
        var lookup = RequiredMetrics.ToDictionary(
            m => m.Value,
            m => Get(series, m).ToDictionary(p => p.Week, p => p.Value));

        var utilisation = BuildUtilisationSeries(series).ToDictionary(p => p.Week, p => p.Value);
        var points = new List<WeeklyPointDto>(range.WeekCount);

        foreach (var week in range.Enumerate())
        {
            decimal Value(MetricCode metric) =>
                lookup[metric.Value].TryGetValue(week, out var v) ? v : 0m;

            points.Add(new WeeklyPointDto(
                week.ToCompactString(),
                Value(MetricCode.EstimatedInfections),
                Value(MetricCode.HospitalAdmissions),
                Value(MetricCode.IcuAdmissions),
                utilisation.TryGetValue(week, out var u) ? u : 0m));
        }

        return points;
    }

    private BedUtilisationDto BuildBedUtilisation(
        Dictionary<string, List<SeriesPoint>> series, EpiWeekRange range)
    {
        var latest = LatestWeek(series) ?? range.To;

        decimal At(MetricCode metric) =>
            Get(series, metric).LastOrDefault(p => p.Week == latest)?.Value ?? 0m;

        var covid = At(MetricCode.IcuBedsCovid);
        var nonCovid = At(MetricCode.IcuBedsNonCovid);
        var capacity = At(MetricCode.IcuBedsTotalCapacity);
        var empty = Math.Max(0m, capacity - covid - nonCovid);

        decimal Pct(decimal part) => capacity <= 0m ? 0m : Math.Round(part / capacity * 100m, 1);

        return new BedUtilisationDto(
            UtilisationPct: Pct(covid + nonCovid),
            TotalCapacity: capacity,
            WeekLabel: $"Week of {FormatWeekSpan(latest, withYear: true)}",
            Segments:
            [
                new BedSegmentDto("COVID Beds", covid, Pct(covid), "infections"),
                new BedSegmentDto("Non-COVID Beds", nonCovid, Pct(nonCovid), "hospital"),
                new BedSegmentDto("Empty Beds", empty, Pct(empty), "utilisation"),
            ]);
    }

    private static IReadOnlyList<InsightDto> BuildInsights(Dictionary<string, List<SeriesPoint>> series)
    {
        var insights = new List<InsightDto>(5);

        var peakInfections = SeriesAnalytics.Peak(Get(series, MetricCode.EstimatedInfections));
        if (peakInfections is not null)
            insights.Add(new InsightDto("peak-infections", "infections",
                "Peak infections occurred in",
                $"Week {peakInfections.Week.ToCompactString()} ({FormatWeekSpan(peakInfections.Week)})"));

        var peakHospital = SeriesAnalytics.Peak(Get(series, MetricCode.HospitalAdmissions));
        if (peakHospital is not null)
            insights.Add(new InsightDto("peak-hospital", "hospital",
                "Peak hospital admissions in",
                $"Week {peakHospital.Week.ToCompactString()} ({FormatWeekSpan(peakHospital.Week)})"));

        var utilisation = BuildUtilisationSeries(series);
        var peakUtilisation = SeriesAnalytics.Peak(utilisation);
        if (peakUtilisation is not null)
            insights.Add(new InsightDto("peak-icu", "icu",
                "Peak ICU utilisation in",
                $"Week {peakUtilisation.Week.ToCompactString()} ({FormatWeekSpan(peakUtilisation.Week)})"));

        if (utilisation.Count > 0)
            insights.Add(new InsightDto("avg-icu", "utilisation",
                "Average ICU utilisation",
                $"{SeriesAnalytics.Average(utilisation):0.0}% during selected period"));

        var lag = SeriesAnalytics.BestLag(SeriesAnalytics.CrossCorrelation(
            Get(series, MetricCode.EstimatedInfections),
            Get(series, MetricCode.IcuAdmissions)));

        if (lag is { Coefficient: >= 0.5 })
            insights.Add(new InsightDto("icu-lag", "icu",
                lag.LagWeeks == 0
                    ? "ICU admissions track infections"
                    : $"ICU admissions lag infections by {lag.LagWeeks} week{(lag.LagWeeks == 1 ? "" : "s")}",
                $"Correlation {lag.Coefficient:0.00} across the selected period"));

        return insights;
    }

    private static IReadOnlyList<DatasetCoverageDto> BuildCoverage(IReadOnlyList<DatasetCoverageInfo> coverage)
    {
        string[] tones = ["infections", "hospital", "utilisation", "latest", "icu"];

        return coverage.Select((c, i) => new DatasetCoverageDto(
            c.DatasetId,
            c.Name,
            c.From is null || c.To is null
                ? "No data"
                : $"{FormatMonth(c.From.Value)} - {FormatMonth(c.To.Value)}",
            $"{c.RecordCount:N0} week{(c.RecordCount == 1 ? "" : "s")}",
            tones[i % tones.Length])).ToList();
    }

    private static EpiWeek? LatestWeek(Dictionary<string, List<SeriesPoint>> series)
    {
        EpiWeek? latest = null;
        foreach (var list in series.Values)
        {
            if (list.Count == 0) continue;
            var last = list[^1].Week;
            if (latest is null || last > latest.Value) latest = last;
        }
        return latest;
    }

    private static string FormatCount(decimal value) => value switch
    {
        >= 1_000_000m => $"{value / 1_000_000m:0.##}M",
        _ => value.ToString("N0", CultureInfo.InvariantCulture),
    };

    private static string FormatWeekSpan(EpiWeek week, bool withYear = false)
    {
        var start = week.StartDate;
        var end = week.EndDate;
        var suffix = withYear ? $" {end.Year}" : string.Empty;

        return start.Month == end.Month
            ? $"{start.Day} - {end.Day} {end:MMM}{suffix}"
            : $"{start.Day} {start:MMM} - {end.Day} {end:MMM}{suffix}";
    }

    private static string FormatMonth(EpiWeek week) =>
        week.StartDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);

    private static string FormatRangeLabel(EpiWeekRange range) =>
        $"{FormatMonth(range.From)} - {FormatMonth(range.To)}";
}