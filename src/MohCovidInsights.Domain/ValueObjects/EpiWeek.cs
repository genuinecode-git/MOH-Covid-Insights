using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MohCovidInsights.Domain.ValueObjects;

public readonly record struct EpiWeek : IComparable<EpiWeek>, IParsable<EpiWeek>
{
    public const int MinYear = 2020;
    public const int MaxYear = 2100;

    /// <summary>Sortable integer key (<c>YYYYWW</c>) used as the persisted column.</summary>
    public int ToKey() => (Year * 100) + Week;

    public static EpiWeek FromKey(int key) => new(key / 100, key % 100);
    
    public EpiWeek(int year, int week)
    {
        if (year is < MinYear or > MaxYear)
            throw new ArgumentOutOfRangeException(nameof(year), year, $"Year must be between {MinYear} and {MaxYear}.");

        var max = WeeksInYear(year);
        if (week < 1 || week > max)
            throw new ArgumentOutOfRangeException(nameof(week), week, $"Epi year {year} has {max} weeks.");

        Year = year;
        Week = week;
    }

    public int Year { get; }
    public int Week { get; }

    /// <summary>False for <c>default(EpiWeek)</c>, which bypasses the constructor.</summary>
    public bool IsInitialised => Year >= MinYear && Week >= 1;

    public DateOnly StartDate
    {
        get
        {
            EnsureInitialised();
            return FirstDayOfEpiYear(Year).AddDays((Week - 1) * 7);
        }
    }

    public DateOnly EndDate => StartDate.AddDays(6);

    /// <summary>The Sunday that begins week 1 of the given epi year.</summary>
    public static DateOnly FirstDayOfEpiYear(int year)
    {
        var jan1 = new DateOnly(year, 1, 1);
        var offsetToWednesday = ((int)DayOfWeek.Wednesday - (int)jan1.DayOfWeek + 7) % 7;
        var firstWednesday = jan1.AddDays(offsetToWednesday);
        return firstWednesday.AddDays(-3);
    }

    public static int WeeksInYear(int year) =>
        (FirstDayOfEpiYear(year + 1).DayNumber - FirstDayOfEpiYear(year).DayNumber) / 7;

    public static EpiWeek FromDate(DateOnly date)
    {
        var year = date.Year;

        if (date >= FirstDayOfEpiYear(year + 1)) year++;
        else if (date < FirstDayOfEpiYear(year)) year--;

        var week = ((date.DayNumber - FirstDayOfEpiYear(year).DayNumber) / 7) + 1;
        return new EpiWeek(year, week);
    }

    public EpiWeek AddWeeks(int count)
    {
        EnsureInitialised();
        return FromDate(StartDate.AddDays(count * 7));
    }

    public EpiWeek Next() => AddWeeks(1);
    public EpiWeek Previous() => AddWeeks(-1);

    /// <summary>Inclusive count of weeks from this week to <paramref name="other"/>.</summary>
    public int WeeksUntil(EpiWeek other)
    {
        EnsureInitialised();
        return (other.StartDate.DayNumber - StartDate.DayNumber) / 7;
    }

    public override string ToString() => $"{Year}-W{Week:D2}";

    /// <summary>Compact label used by the MOH CSV feeds, e.g. <c>2023-09</c>.</summary>
    public string ToCompactString() => $"{Year}-{Week:D2}";

    public static EpiWeek Parse(string s, IFormatProvider? provider = null) =>
        TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"'{s}' is not a valid epi week. Expected '2023-W09' or '2023-09'.");

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out EpiWeek result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s)) return false;

        var span = s.AsSpan().Trim();
        var dash = span.IndexOf('-');
        if (dash <= 0 || dash == span.Length - 1) return false;

        var yearPart = span[..dash];
        var weekPart = span[(dash + 1)..];
        if (weekPart is ['W' or 'w', ..]) weekPart = weekPart[1..];

        if (!int.TryParse(yearPart, NumberStyles.None, CultureInfo.InvariantCulture, out var year)) return false;
        if (!int.TryParse(weekPart, NumberStyles.None, CultureInfo.InvariantCulture, out var week)) return false;
        if (year is < MinYear or > MaxYear) return false;
        if (week < 1 || week > WeeksInYear(year)) return false;

        result = new EpiWeek(year, week);
        return true;
    }

    public static bool TryParse(string? s, out EpiWeek result) => TryParse(s, null, out result);

    public int CompareTo(EpiWeek other) =>
        Year != other.Year ? Year.CompareTo(other.Year) : Week.CompareTo(other.Week);

    public static bool operator <(EpiWeek a, EpiWeek b) => a.CompareTo(b) < 0;
    public static bool operator >(EpiWeek a, EpiWeek b) => a.CompareTo(b) > 0;
    public static bool operator <=(EpiWeek a, EpiWeek b) => a.CompareTo(b) <= 0;
    public static bool operator >=(EpiWeek a, EpiWeek b) => a.CompareTo(b) >= 0;

    private void EnsureInitialised()
    {
        if (!IsInitialised)
            throw new InvalidOperationException("EpiWeek was not initialised via its constructor.");
    }
}