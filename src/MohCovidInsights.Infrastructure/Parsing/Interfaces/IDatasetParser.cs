using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Infrastructure.Parsing.Interfaces;

/// <summary>Strategy: one configuration per MOH dataset, resolved by keyed DI.</summary>
public interface IDatasetParser
{
    /// <summary>Metrics this parser emits. Used to attribute observations back to a dataset.</summary>
    IReadOnlyCollection<MetricCode> Metrics { get; }

    IReadOnlyList<WeeklyObservation> Parse(ITabularData table);
}