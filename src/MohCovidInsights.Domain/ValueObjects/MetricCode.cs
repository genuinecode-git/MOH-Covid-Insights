namespace MohCovidInsights.Domain.ValueObjects;

/// <summary>Smart enum of the metrics derived from the four MOH datasets.</summary>
public sealed record MetricCode
{
    private static readonly Dictionary<string, MetricCode> Registry = new(StringComparer.OrdinalIgnoreCase);

    public static readonly MetricCode EstimatedInfections =
        Register("estimated_infections", "Estimated Infections", "cases");

    public static readonly MetricCode HospitalAdmissions =
        Register("hospital_admissions", "Hospital Admissions", "admissions");

    public static readonly MetricCode IcuAdmissions =
        Register("icu_admissions", "ICU Admissions", "admissions");

    public static readonly MetricCode AvgHospitalisedCases =
        Register("avg_hospitalised_cases", "Avg Hospitalised Cases", "cases");

    public static readonly MetricCode AvgIcuCases =
        Register("avg_icu_cases", "Avg ICU Cases", "cases");

    public static readonly MetricCode IcuBedsCovid =
        Register("icu_beds_covid", "COVID ICU Beds", "beds");

    public static readonly MetricCode IcuBedsNonCovid =
        Register("icu_beds_non_covid", "Non-COVID ICU Beds", "beds");

    public static readonly MetricCode IcuBedsTotalCapacity =
        Register("icu_beds_total_capacity", "Total ICU Capacity", "beds");
    public static readonly MetricCode IcuBedsEmpty =
        Register("icu_beds_empty", "Empty ICU Beds", "beds");

    public string Value { get; }
    public string DisplayName { get; }
    public string Unit { get; }

    private MetricCode(string value, string displayName, string unit)
    {
        Value = value;
        DisplayName = displayName;
        Unit = unit;
    }

    public static IReadOnlyCollection<MetricCode> All => Registry.Values;

    public static bool TryFrom(string? value, out MetricCode metric)
    {
        metric = null!;
        return value is not null && Registry.TryGetValue(value, out metric!);
    }

    public static MetricCode From(string value) =>
        TryFrom(value, out var m) ? m : throw new ArgumentException($"Unknown metric code '{value}'.", nameof(value));

    private static MetricCode Register(string value, string displayName, string unit)
    {
        var metric = new MetricCode(value, displayName, unit);
        Registry[value] = metric;
        return metric;
    }

    public override string ToString() => Value;
}