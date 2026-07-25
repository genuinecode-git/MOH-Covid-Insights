using MohCovidInsights.Application.Abstractions;
using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Application.Dashboard.Queries;
using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Domain.ValueObjects;

using NSubstitute;

using Shouldly;

namespace MohCovidInsights.Application.Tests;

public class GetDashboardQueryHandlerTests
{
    private readonly IObservationRepository _observations = Substitute.For<IObservationRepository>();
    private readonly IDatasetCatalogue _catalogue = Substitute.For<IDatasetCatalogue>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private GetDashboardQueryHandler CreateSut()
    {
        _clock.UtcNow.Returns(new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.Zero));
        _catalogue.GetCoverageAsync(Arg.Any<CancellationToken>())
            .Returns([new DatasetCoverageInfo("d_infections", "Weekly Estimated Infections",
                new EpiWeek(2023, 1), new EpiWeek(2024, 4), 56,
                new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.Zero), "infections")]);

        return new GetDashboardQueryHandler(_observations, _catalogue, _clock);
    }

    private void GivenObservations(params WeeklyObservation[] rows) =>
        _observations.GetAsync(Arg.Any<EpiWeekRange>(), Arg.Any<IReadOnlyCollection<MetricCode>>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(rows);

    private static WeeklyObservation Obs(int week, MetricCode metric, decimal value) =>
        new(new EpiWeek(2023, week), metric, value);

    [Fact]
    public async Task ReturnsValidationError_ForMalformedEpiWeek()
    {
        var result = await CreateSut().HandleAsync(new GetDashboardQuery("not-a-week", "2024-W04"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation");
    }

    [Fact]
    public async Task ReturnsValidationError_WhenStartIsAfterEnd()
    {
        var result = await CreateSut().HandleAsync(new GetDashboardQuery("2024-W04", "2023-W01"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation");
    }

    [Fact]
    public async Task ReturnsNotFound_WhenNoObservationsExist()
    {
        GivenObservations();

        var result = await CreateSut().HandleAsync(new GetDashboardQuery("2023-W01", "2023-W10"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("not_found");
    }

    [Fact]
    public async Task ProducesSixKpis_WithPeakWeekMatchingTheData()
    {
        GivenObservations(
            Obs(1, MetricCode.EstimatedInfections, 10_000),
            Obs(2, MetricCode.EstimatedInfections, 58_300),
            Obs(3, MetricCode.EstimatedInfections, 20_000));

        var result = await CreateSut().HandleAsync(new GetDashboardQuery("2023-W01", "2023-W03"));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Kpis.Count.ShouldBe(6);
        result.Value.Kpis.Single(k => k.Id == "peak-infection-week").Value.ShouldBe("2023-02");
    }

    [Fact]
    public async Task PadsWeeklySeries_ForEveryWeekInRange()
    {
        GivenObservations(Obs(1, MetricCode.EstimatedInfections, 10_000));

        var result = await CreateSut().HandleAsync(new GetDashboardQuery("2023-W01", "2023-W05"));

        result.Value.Weekly.Count.ShouldBe(5);
        result.Value.Weekly[^1].EstimatedInfections.ShouldBe(0m);
        result.Value.Weekly[0].EpiWeek.ShouldBe("2023-01");
    }

    [Fact]
    public async Task ComputesBedUtilisation_FromLatestWeekCapacity()
    {
        GivenObservations(
            Obs(1, MetricCode.IcuBedsCovid, 312),
            Obs(1, MetricCode.IcuBedsNonCovid, 707),
            Obs(1, MetricCode.IcuBedsTotalCapacity, 1396));

        var result = await CreateSut().HandleAsync(new GetDashboardQuery("2023-W01", "2023-W01"));

        var beds = result.Value.BedUtilisation;
        beds.TotalCapacity.ShouldBe(1396m);
        beds.UtilisationPct.ShouldBe(73.0m);
        beds.Segments.Single(s => s.Label == "Empty Beds").Beds.ShouldBe(377m);
    }

    [Fact]
    public async Task FormatsTotalsOverAMillionCompactly()
    {
        GivenObservations(
            Obs(1, MetricCode.EstimatedInfections, 760_000),
            Obs(2, MetricCode.EstimatedInfections, 760_000));

        var result = await CreateSut().HandleAsync(new GetDashboardQuery("2023-W01", "2023-W02"));

        result.Value.Kpis.Single(k => k.Id == "total-infections").Value.ShouldBe("1.52M");
    }
}