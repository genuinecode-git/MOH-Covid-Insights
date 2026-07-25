using MohCovidInsights.Domain.ValueObjects;

using Shouldly;

namespace MohCovidInsights.Domain.Tests;

public class EpiWeekRangeTests
{
    [Fact]
    public void Enumerate_CoversFullDashboardRange()
    {
        var range = new EpiWeekRange(new EpiWeek(2023, 1), new EpiWeek(2024, 4));

        range.WeekCount.ShouldBe(56);
        range.Enumerate().Count().ShouldBe(56);
    }

    [Fact]
    public void PreviousPeriod_IsEqualLengthAndImmediatelyBefore()
    {
        var range = new EpiWeekRange(new EpiWeek(2023, 10), new EpiWeek(2023, 19));
        var previous = range.PreviousPeriod();

        previous.WeekCount.ShouldBe(range.WeekCount);
        previous.To.ShouldBe(new EpiWeek(2023, 9));
        previous.From.ShouldBe(new EpiWeek(2022, 52));
    }

    [Fact]
    public void TryCreate_FailsWhenStartIsAfterEnd()
    {
        var result = EpiWeekRange.TryCreate("2024-W04", "2023-W01");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation");
    }

    [Fact]
    public void TryCreate_SucceedsForValidInput()
    {
        var result = EpiWeekRange.TryCreate("2023-W01", "2024-W04");

        result.IsSuccess.ShouldBeTrue();
        result.Value.WeekCount.ShouldBe(56);
    }

    [Fact]
    public void Contains_RespectsInclusiveBounds()
    {
        var range = new EpiWeekRange(new EpiWeek(2023, 5), new EpiWeek(2023, 10));

        range.Contains(new EpiWeek(2023, 5)).ShouldBeTrue();
        range.Contains(new EpiWeek(2023, 10)).ShouldBeTrue();
        range.Contains(new EpiWeek(2023, 11)).ShouldBeFalse();
    }
}