namespace MohCovidInsights.Api.Models;

public sealed class DashboardRequest
{
    [FromQuery(Name = "from")]
    public string From { get; init; } = "2023-W01";

    [FromQuery(Name = "to")]
    public string To { get; init; } = "2024-W04";

    [FromQuery(Name = "ageGroup")]
    public string AgeGroup { get; init; } = "All";

    [FromQuery(Name = "clinicalStatus")]
    public string ClinicalStatus { get; init; } = "All";
}