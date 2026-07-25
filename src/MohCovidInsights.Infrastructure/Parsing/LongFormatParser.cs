using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Infrastructure.Parsing;

public sealed class LongFormatParser(LongFormatSpec spec) : IDatasetParser
{
    public IReadOnlyCollection<MetricCode> Metrics
    {
        get
        {
            var all = new List<MetricCode>(spec.StatusMap.Values);
            if (spec.DefaultMetric is not null) all.Add(spec.DefaultMetric);
            if (spec.TotalMetric is not null) all.Add(spec.TotalMetric);
            return all;
        }
    }

    public IReadOnlyList<WeeklyObservation> Parse(ITabularData table)
    {
        var weekIndex = table.Require(spec.WeekField);
        var yearIndex = table.IndexOf(spec.YearField);
        var valueIndex = table.Require(spec.ValueField);
        var statusIndex = spec.StatusField is null ? -1 : table.Require(spec.StatusField);
        var dimensionIndex = spec.DimensionField is null ? -1 : table.Require(spec.DimensionField);

        var results = new List<WeeklyObservation>();
        var dimensionTotals = new Dictionary<(int Week, string Metric), decimal>();
        var statusTotals = new Dictionary<(int Week, string Dimension), decimal>();

        foreach (var row in table.Rows)
        {
            if (!FieldReader.TryReadEpiWeek(row, weekIndex, yearIndex, out var week)) continue;
            if (!FieldReader.TryReadDecimal(row, valueIndex, out var value)) continue;
            if (!TryResolveMetric(row, statusIndex, out var metric)) continue;

            var dimension = dimensionIndex < 0
                ? AgeGroup.All
                : AgeGroup.Normalise(row[dimensionIndex]);

            results.Add(new WeeklyObservation(week, metric, value, dimension));

            if (spec.EmitDimensionTotal)
            {
                var key = (week.ToKey(), metric.Value);
                dimensionTotals[key] = dimensionTotals.GetValueOrDefault(key) + value;
            }

            if (spec.TotalMetric is not null)
            {
                var key = (week.ToKey(), dimension);
                statusTotals[key] = statusTotals.GetValueOrDefault(key) + value;
            }
        }

        foreach (var ((weekKey, metricCode), total) in dimensionTotals)
        {
            results.Add(new WeeklyObservation(
                EpiWeek.FromKey(weekKey), MetricCode.From(metricCode),
                Math.Round(total, 2), AgeGroup.All));
        }

        foreach (var ((weekKey, dimension), total) in statusTotals)
        {
            results.Add(new WeeklyObservation(
                EpiWeek.FromKey(weekKey), spec.TotalMetric!,
                Math.Round(total, 2), dimension));
        }

        if (results.Count == 0 && table.Rows.Count > 0)
            throw new InvalidOperationException(
                $"Read {table.Rows.Count} rows but produced no observations. " +
                $"Check the status mapping. Fields: {string.Join(", ", table.Fields)}");

        return results;
    }

    private bool TryResolveMetric(string[] row, int statusIndex, out MetricCode metric)
    {
        if (statusIndex < 0)
        {
            metric = spec.DefaultMetric
                     ?? throw new InvalidOperationException("Spec has neither a status field nor a default metric.");
            return true;
        }

        var status = row[statusIndex].Trim();

        foreach (var (key, value) in spec.StatusMap)
        {
            if (key.Equals(status, StringComparison.OrdinalIgnoreCase))
            {
                metric = value;
                return true;
            }
        }

        metric = null!;
        return false;
    }
}
