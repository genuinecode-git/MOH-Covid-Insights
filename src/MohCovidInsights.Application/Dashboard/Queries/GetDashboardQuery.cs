using MohCovidInsights.Application.Dashboard.Dtos;

namespace MohCovidInsights.Application.Dashboard.Queries;

public sealed record GetDashboardQuery(
    string From,
    string To,
    string AgeGroup = "All",
    string ClinicalStatus = "All") : IQuery<DashboardDto>;