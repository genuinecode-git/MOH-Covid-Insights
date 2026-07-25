namespace MohCovidInsights.Application.HospitalisationCases.Dtos;

public sealed record HospitalisationCaseRowDto(
    int EpiYear,
    string EpiWeek,
    DateOnly WeekStart,
    string ClinicalStatus,
    string AgeGroup,
    string AgeGroupLabel,
    decimal AverageDailyCases);

public sealed record HospitalisationCasesDto(
    string From,
    string To,
    IReadOnlyList<string> ClinicalStatuses,
    IReadOnlyList<string> AgeGroups,
    IReadOnlyList<HospitalisationCaseRowDto> Rows);