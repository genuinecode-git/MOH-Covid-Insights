namespace MohCovidInsights.Application.Dashboard.Dtos;

public sealed record KpiTrendDto(decimal ChangePct, string Direction);

public sealed record KpiDto(
    string Id,
    string Label,
    string Value,
    string Tone,
    KpiTrendDto? Trend = null,
    string? Caption = null);

public sealed record WeeklyPointDto(
    string EpiWeek,
    decimal EstimatedInfections,
    decimal HospitalAdmissions,
    decimal IcuAdmissions,
    decimal IcuUtilisationPct);

public sealed record BedSegmentDto(string Label, decimal Beds, decimal Pct, string Tone);

public sealed record BedUtilisationDto(
    decimal UtilisationPct,
    decimal TotalCapacity,
    string WeekLabel,
    IReadOnlyList<BedSegmentDto> Segments);

public sealed record InsightDto(string Id, string Tone, string Headline, string Detail);

public sealed record DatasetCoverageDto(string Id, string Name, string Coverage, string Volume, string Tone);

public sealed record DashboardDto(
    IReadOnlyList<KpiDto> Kpis,
    IReadOnlyList<WeeklyPointDto> Weekly,
    BedUtilisationDto BedUtilisation,
    IReadOnlyList<InsightDto> Insights,
    IReadOnlyList<DatasetCoverageDto> Coverage,
    string LastUpdated,
    string RangeLabel);