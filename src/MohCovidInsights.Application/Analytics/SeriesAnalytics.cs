using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Application.Analytics;

public sealed record SeriesPoint(EpiWeek Week, decimal Value);

public sealed record PeakResult(EpiWeek Week, decimal Value);

public sealed record LagCorrelation(int LagWeeks, double Coefficient);

/// <summary>
/// Pure functions over epi-week series. No I/O, no DI — trivially unit-testable
/// and the place every number on the dashboard ultimately comes from.
/// </summary>
public static class SeriesAnalytics
{
    public static PeakResult? Peak(IReadOnlyList<SeriesPoint> series)
    {
        if (series.Count == 0) return null;

        var best = series[0];
        foreach (var p in series)
            if (p.Value > best.Value) best = p;

        return new PeakResult(best.Week, best.Value);
    }

    public static decimal Total(IReadOnlyList<SeriesPoint> series) =>
        series.Sum(p => p.Value);

    public static decimal Average(IReadOnlyList<SeriesPoint> series) =>
        series.Count == 0 ? 0m : series.Sum(p => p.Value) / series.Count;

    /// <summary>Percentage change from <paramref name="previous"/> to <paramref name="current"/>.</summary>
    public static decimal? PercentChange(decimal previous, decimal current)
    {
        return previous == 0m ? null : Math.Round((current - previous) / Math.Abs(previous) * 100m, 1);
    }

    /// <summary>Week-over-week percentage change, used by the dashboard's "% change" toggle.</summary>
    public static IReadOnlyList<SeriesPoint> ToWeekOverWeekChange(IReadOnlyList<SeriesPoint> series)
    {
        if (series.Count < 2) return [];

        var result = new List<SeriesPoint>(series.Count - 1);
        for (var i = 1; i < series.Count; i++)
        {
            var change = PercentChange(series[i - 1].Value, series[i].Value) ?? 0m;
            result.Add(new SeriesPoint(series[i].Week, change));
        }
        return result;
    }

    public static double PearsonCorrelation(IReadOnlyList<decimal> a, IReadOnlyList<decimal> b)
    {
        var n = Math.Min(a.Count, b.Count);
        if (n < 3) return 0d;

        double meanA = 0, meanB = 0;
        for (var i = 0; i < n; i++) { meanA += (double)a[i]; meanB += (double)b[i]; }
        meanA /= n; meanB /= n;

        double cov = 0, varA = 0, varB = 0;
        for (var i = 0; i < n; i++)
        {
            var da = (double)a[i] - meanA;
            var db = (double)b[i] - meanB;
            cov += da * db;
            varA += da * da;
            varB += db * db;
        }

        var denominator = Math.Sqrt(varA * varB);
        return denominator == 0d ? 0d : cov / denominator;
    }

    /// <summary>
    /// Correlates a driver series against a response series shifted forward by 0..maxLag weeks.
    /// MOH notes that hospitalisation and ICU indicators lag infections, because symptoms
    /// take time to develop — this quantifies that lag from the data itself.
    /// </summary>
    public static IReadOnlyList<LagCorrelation> CrossCorrelation(
        IReadOnlyList<SeriesPoint> driver,
        IReadOnlyList<SeriesPoint> response,
        int maxLag = 6)
    {
        var results = new List<LagCorrelation>(maxLag + 1);
        var byWeek = response.ToDictionary(p => p.Week, p => p.Value);

        for (var lag = 0; lag <= maxLag; lag++)
        {
            var driverValues = new List<decimal>();
            var responseValues = new List<decimal>();

            foreach (var point in driver)
            {
                if (!byWeek.TryGetValue(point.Week.AddWeeks(lag), out var responseValue)) continue;
                driverValues.Add(point.Value);
                responseValues.Add(responseValue);
            }

            results.Add(new LagCorrelation(lag, Math.Round(PearsonCorrelation(driverValues, responseValues), 3)));
        }

        return results;
    }

    public static LagCorrelation? BestLag(IReadOnlyList<LagCorrelation> correlations)
    {
        if (correlations.Count == 0) return null;

        var best = correlations[0];
        foreach (var c in correlations)
            if (c.Coefficient > best.Coefficient) best = c;

        return best;
    }
}