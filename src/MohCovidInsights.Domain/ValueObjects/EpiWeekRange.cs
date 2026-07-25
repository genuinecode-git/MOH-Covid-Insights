using MohCovidInsights.Domain.Common;

namespace MohCovidInsights.Domain.ValueObjects;

public readonly record struct EpiWeekRange
{
    public EpiWeekRange(EpiWeek from, EpiWeek to)
    {
        if (from > to)
            throw new ArgumentException($"Range start {from} is after end {to}.", nameof(from));

        From = from;
        To = to;
    }

    public EpiWeek From { get; }
    public EpiWeek To { get; }

    public int WeekCount => From.WeeksUntil(To) + 1;

    public bool Contains(EpiWeek week) => week >= From && week <= To;

    public IEnumerable<EpiWeek> Enumerate()
    {
        var current = From;
        while (current <= To)
        {
            yield return current;
            current = current.Next();
        }
    }

    /// <summary>The equally sized range immediately preceding this one, for period-over-period comparison.</summary>
    public EpiWeekRange PreviousPeriod()
    {
        var count = WeekCount;
        return new EpiWeekRange(From.AddWeeks(-count), From.Previous());
    }

    public static Result<EpiWeekRange> TryCreate(string? from, string? to)
    {
        if (!EpiWeek.TryParse(from, out var start))
            return Error.Validation($"'{from}' is not a valid epi week.");

        if (!EpiWeek.TryParse(to, out var end))
            return Error.Validation($"'{to}' is not a valid epi week.");

        return start > end ? (Result<EpiWeekRange>)Error.Validation($"Range start {start} is after end {end}.") : (Result<EpiWeekRange>)new EpiWeekRange(start, end);
    }

    public override string ToString() => $"{From} - {To}";
}