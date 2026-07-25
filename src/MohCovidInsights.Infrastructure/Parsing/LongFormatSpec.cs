using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Infrastructure.Parsing;


public sealed record LongFormatSpec
{
    public string WeekField { get; init; } = "epi_week";
    public string YearField { get; init; } = "epi_year";
    public string ValueField { get; init; } = "count";

    public string? StatusField { get; init; }

    public IReadOnlyDictionary<string, MetricCode> StatusMap { get; init; } = new Dictionary<string, MetricCode>();

    public MetricCode? DefaultMetric { get; init; }

    public string? DimensionField { get; init; }

    public bool EmitDimensionTotal { get; init; }

    public MetricCode? TotalMetric { get; init; }
}
