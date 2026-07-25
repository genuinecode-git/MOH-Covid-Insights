using MohCovidInsights.Application.Analytics;
using MohCovidInsights.Domain.ValueObjects;

using Shouldly;

namespace MohCovidInsights.Application.Tests;

public class SeriesAnalyticsTests
{
    private static List<SeriesPoint> Series(params decimal[] values) =>
        values.Select((v, i) => new SeriesPoint(new EpiWeek(2023, i + 1), v)).ToList();

    [Fact]
    public void Peak_ReturnsHighestValueAndItsWeek()
    {
        var peak = SeriesAnalytics.Peak(Series(10, 45, 30, 12));

        peak!.Value.ShouldBe(45m);
        peak.Week.ShouldBe(new EpiWeek(2023, 2));
    }

    [Fact]
    public void Peak_ReturnsNullForEmptySeries() =>
        SeriesAnalytics.Peak([]).ShouldBeNull();

    [Fact]
    public void PercentChange_HandlesIncreaseDecreaseAndZeroBase()
    {
        SeriesAnalytics.PercentChange(100m, 112.4m).ShouldBe(12.4m);
        SeriesAnalytics.PercentChange(100m, 80m).ShouldBe(-20m);
        SeriesAnalytics.PercentChange(0m, 50m).ShouldBeNull();
    }

    [Fact]
    public void ToWeekOverWeekChange_DropsFirstWeek()
    {
        var result = SeriesAnalytics.ToWeekOverWeekChange(Series(100, 150, 75));

        result.Count.ShouldBe(2);
        result[0].Value.ShouldBe(50m);
        result[1].Value.ShouldBe(-50m);
    }

    [Fact]
    public void PearsonCorrelation_IsOneForPerfectlyLinearData() =>
        SeriesAnalytics.PearsonCorrelation([1m, 2m, 3m, 4m], [2m, 4m, 6m, 8m]).ShouldBe(1d, 0.0001);

    [Fact]
    public void PearsonCorrelation_IsNegativeOneForInverseData() =>
        SeriesAnalytics.PearsonCorrelation([1m, 2m, 3m, 4m], [8m, 6m, 4m, 2m]).ShouldBe(-1d, 0.0001);

    [Fact]
    public void PearsonCorrelation_IsZeroForConstantSeries() =>
        SeriesAnalytics.PearsonCorrelation([1m, 2m, 3m], [5m, 5m, 5m]).ShouldBe(0d);

    [Fact]
    public void CrossCorrelation_DetectsKnownTwoWeekLag()
    {
        var driver = Series(10, 20, 40, 80, 40, 20, 10, 20, 40, 80, 40, 20);
        var response = Series(0, 0, 10, 20, 40, 80, 40, 20, 10, 20, 40, 80);

        var best = SeriesAnalytics.BestLag(SeriesAnalytics.CrossCorrelation(driver, response));

        best!.LagWeeks.ShouldBe(2);
        best.Coefficient.ShouldBeGreaterThan(0.95);
    }

    [Fact]
    public void CrossCorrelation_ReturnsOneEntryPerLag() =>
        SeriesAnalytics.CrossCorrelation(Series(1, 2, 3, 4, 5), Series(1, 2, 3, 4, 5), maxLag: 4)
            .Count.ShouldBe(5);
}