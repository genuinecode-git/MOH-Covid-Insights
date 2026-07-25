using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Domain.Entities;

public sealed partial class WeeklyObservation
{
    private WeeklyObservation() { }

    public WeeklyObservation(EpiWeek epiWeek, MetricCode metric, decimal value, string dimension = Dimensions.All)
    {
        ArgumentNullException.ThrowIfNull(metric);
        ArgumentException.ThrowIfNullOrWhiteSpace(dimension);

        EpiWeekKey = epiWeek.ToKey();
        MetricCode = metric.Value;
        Value = value;
        Dimension = dimension;
    }

    /// <summary>Persisted as <c>YYYYWW</c> so range filters translate to SQL and stay index-friendly.</summary>
    public int EpiWeekKey { get; private set; }

    public string MetricCode { get; private set; } = null!;
    public string Dimension { get; private set; } = Dimensions.All;
    public decimal Value { get; private set; }

    public EpiWeek EpiWeek => EpiWeek.FromKey(EpiWeekKey);
    public MetricCode Metric => ValueObjects.MetricCode.From(MetricCode);    
}