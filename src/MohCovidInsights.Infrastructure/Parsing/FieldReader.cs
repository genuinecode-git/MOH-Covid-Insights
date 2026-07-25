using System.Globalization;
using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Infrastructure.Parsing;

public static class FieldReader
{
    public static bool TryReadDecimal(string[] row, int index, out decimal value)
    {
        value = 0m;
        if (index < 0 || index >= row.Length) return false;

        var raw = row[index].Replace(",", string.Empty).Replace("%", string.Empty).Trim();
        if (raw.Length == 0 || raw is "-" or "NA" or "na" or "null") return false;

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryReadEpiWeek(string[] row, int weekIndex, int yearIndex, out EpiWeek week)
    {
        week = default;
        if (weekIndex < 0 || weekIndex >= row.Length) return false;

        var raw = row[weekIndex].Trim().Replace(" ", "-");
        if (EpiWeek.TryParse(raw, out week)) return true;

        if (yearIndex < 0 || yearIndex >= row.Length) return false;
        if (!int.TryParse(row[yearIndex].Trim(), out var year)) return false;
        if (!int.TryParse(raw.TrimStart('W', 'w'), out var weekNumber)) return false;
        if (weekNumber < 1 || weekNumber > EpiWeek.WeeksInYear(year)) return false;

        week = new EpiWeek(year, weekNumber);
        return true;
    }
}
