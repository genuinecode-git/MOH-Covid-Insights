using Microsoft.Extensions.DependencyInjection;

using MohCovidInsights.Application.Abstractions;
using MohCovidInsights.Application.Dashboard.Dtos;
using MohCovidInsights.Application.Dashboard.Queries;
using MohCovidInsights.Application.HospitalisationCases.Dtos;
using MohCovidInsights.Application.HospitalisationCases.Queries;

namespace MohCovidInsights.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IQueryHandler<GetDashboardQuery, DashboardDto>, GetDashboardQueryHandler>();
        services.AddScoped<IQueryHandler<GetHospitalisationCasesQuery, HospitalisationCasesDto>, GetHospitalisationCasesQueryHandler>();
        return services;
    }
}