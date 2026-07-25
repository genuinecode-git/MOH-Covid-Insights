using MohCovidInsights.Application.HospitalisationCases.Dtos;
using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Application.HospitalisationCases.Queries;

public sealed record GetHospitalisationCasesQuery(
    string From,
    string To,
    string? ClinicalStatus = null,
    string? AgeGroup = null) : IQuery<HospitalisationCasesDto>;

public sealed class GetHospitalisationCasesQueryHandler(IObservationRepository observations)
    : IQueryHandler<GetHospitalisationCasesQuery, HospitalisationCasesDto>
{
    private const string Hospitalised = "Hospitalised";
    private const string Icu = "ICU";

    private static readonly MetricCode[] Metrics =
        [MetricCode.AvgHospitalisedCases, MetricCode.AvgIcuCases];

    public async Task<Result<HospitalisationCasesDto>> HandleAsync(
        GetHospitalisationCasesQuery query, CancellationToken ct = default)
    {
        var rangeResult = EpiWeekRange.TryCreate(query.From, query.To);
        if (rangeResult.IsFailure) return rangeResult.Error;

        var range = rangeResult.Value;
        var rows = await observations.GetAllDimensionsAsync(range, Metrics, ct);

        if (rows.Count == 0)
            return Error.NotFound($"No hospitalisation cases recorded for {range}.");

        var projected = rows
            .Where(o => o.Dimension != AgeGroup.All)
            .Select(o =>
            {
                var week = o.EpiWeek;
                return new HospitalisationCaseRowDto(
                    week.Year,
                    week.ToCompactString(),
                    week.StartDate,
                    o.MetricCode == MetricCode.AvgIcuCases.Value ? Icu : Hospitalised,
                    o.Dimension,
                    AgeGroup.Display(o.Dimension),
                    o.Value);
            })
            .Where(r => Matches(r.ClinicalStatus, query.ClinicalStatus))
            .Where(r => Matches(r.AgeGroup, query.AgeGroup))
            .OrderBy(r => r.EpiWeek, StringComparer.Ordinal)
            .ThenBy(r => r.ClinicalStatus, StringComparer.Ordinal)
            .ThenBy(r => r.AgeGroup, StringComparer.Ordinal)
            .ToList();

        return projected.Count == 0
            ? (Result<HospitalisationCasesDto>)Error.NotFound("No rows match the selected filters.")
            : (Result<HospitalisationCasesDto>)new HospitalisationCasesDto(
            range.From.ToString(),
            range.To.ToString(),
            [Hospitalised, Icu],
            [.. projected.Select(r => r.AgeGroup).Distinct().Order(StringComparer.Ordinal)],
            projected);
    }

    private static bool Matches(string actual, string? filter) =>
        string.IsNullOrWhiteSpace(filter)
        || filter.Equals("All", StringComparison.OrdinalIgnoreCase)
        || actual.Equals(filter, StringComparison.OrdinalIgnoreCase);
}