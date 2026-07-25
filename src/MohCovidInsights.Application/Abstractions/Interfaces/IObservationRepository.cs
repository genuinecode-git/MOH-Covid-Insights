using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Application.Abstractions.Interfaces;

public interface IObservationRepository
{
    Task<IReadOnlyList<WeeklyObservation>> GetAsync(
        EpiWeekRange range,
        IReadOnlyCollection<MetricCode> metrics,
        string dimension,
        CancellationToken ct = default);

    Task<EpiWeek?> GetLatestWeekAsync(CancellationToken ct = default);

    Task<IReadOnlyList<WeeklyObservation>> GetAllDimensionsAsync(
        EpiWeekRange range,
        IReadOnlyCollection<MetricCode> metrics,
        CancellationToken ct = default);
}
