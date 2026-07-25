using MohCovidInsights.Domain.ValueObjects;

using Shouldly;

namespace MohCovidInsights.Domain.Tests;

public class EpiWeekTests
{
    [Theory]
    [InlineData(2023, 1, "2023-01-01", "2023-01-07")]
    [InlineData(2023, 9, "2023-02-26", "2023-03-04")]
    [InlineData(2023, 10, "2023-03-05", "2023-03-11")]
    [InlineData(2023, 52, "2023-12-24", "2023-12-30")]
    [InlineData(2024, 1, "2023-12-31", "2024-01-06")]
    [InlineData(2024, 4, "2024-01-21", "2024-01-27")]
    public void StartAndEndDates_MatchMohCalendar(int year, int week, string start, string end)
    {
        var epiWeek = new EpiWeek(year, week);

        epiWeek.StartDate.ShouldBe(DateOnly.Parse(start));
        epiWeek.EndDate.ShouldBe(DateOnly.Parse(end));
    }

    [Fact]
    public void EveryWeek_StartsOnSunday()
    {
        foreach (var week in new EpiWeekRange(new EpiWeek(2023, 1), new EpiWeek(2024, 4)).Enumerate())
            week.StartDate.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
    }

    [Theory]
    [InlineData(2020, 53)]
    [InlineData(2023, 52)]
    [InlineData(2024, 52)]
    [InlineData(2025, 53)]
    public void WeeksInYear_HandlesLongYears(int year, int expected) =>
        EpiWeek.WeeksInYear(year).ShouldBe(expected);

    [Fact]
    public void Next_RollsOverYearBoundary()
    {
        new EpiWeek(2023, 52).Next().ShouldBe(new EpiWeek(2024, 1));
        new EpiWeek(2024, 1).Previous().ShouldBe(new EpiWeek(2023, 52));
    }

    [Fact]
    public void FromDate_IsInverseOfStartDate()
    {
        foreach (var week in new EpiWeekRange(new EpiWeek(2022, 1), new EpiWeek(2025, 10)).Enumerate())
        {
            EpiWeek.FromDate(week.StartDate).ShouldBe(week);
            EpiWeek.FromDate(week.EndDate).ShouldBe(week);
        }
    }

    [Theory]
    [InlineData("2023-W09", 2023, 9)]
    [InlineData("2023-09", 2023, 9)]
    [InlineData(" 2024-w01 ", 2024, 1)]
    public void TryParse_AcceptsBothFormats(string input, int year, int week)
    {
        EpiWeek.TryParse(input, out var result).ShouldBeTrue();
        result.ShouldBe(new EpiWeek(year, week));
    }

    [Theory]
    [InlineData("")]
    [InlineData("2023")]
    [InlineData("2023-W00")]
    [InlineData("2023-W53")]
    [InlineData("1999-W01")]
    [InlineData("abc-W01")]
    public void TryParse_RejectsInvalidInput(string input) =>
        EpiWeek.TryParse(input, out _).ShouldBeFalse();

    [Fact]
    public void ToString_UsesIsoLikeFormat()
    {
        new EpiWeek(2023, 9).ToString().ShouldBe("2023-W09");
        new EpiWeek(2023, 9).ToCompactString().ShouldBe("2023-09");
    }

    [Fact]
    public void Constructor_RejectsWeekBeyondYearLength() =>
        Should.Throw<ArgumentOutOfRangeException>(() => new EpiWeek(2023, 53));

    [Fact]
    public void Default_IsNotInitialised()
    {
        default(EpiWeek).IsInitialised.ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => default(EpiWeek).StartDate);
    }

    [Fact]
    public void Comparison_OrdersChronologically()
    {
        (new EpiWeek(2023, 52) < new EpiWeek(2024, 1)).ShouldBeTrue();
        (new EpiWeek(2023, 10) > new EpiWeek(2023, 9)).ShouldBeTrue();
    }
}