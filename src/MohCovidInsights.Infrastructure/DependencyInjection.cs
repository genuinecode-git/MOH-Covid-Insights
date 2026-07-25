using Microsoft.Extensions.Configuration;

using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Infrastructure.DataGovSg;
using MohCovidInsights.Infrastructure.Parsing;
using MohCovidInsights.Infrastructure.Persistence;
using MohCovidInsights.Infrastructure.Repositories;
using MohCovidInsights.Infrastructure.Sync;

namespace MohCovidInsights.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DataGovSgOptions>()
            .Bind(configuration.GetSection(DataGovSgOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var provider = configuration.GetValue("Database:Provider", "Sqlite");
        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Data Source=mohcovid.db";

        services.AddDbContext<AppDbContext>(o =>
        {
            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                o.UseNpgsql(connectionString);
            else
                o.UseSqlite(connectionString);
        });

        services.AddHttpClient<IDataGovSgClient, DataGovSgClient>((sp, http) =>
        {
            var options = configuration.GetSection(DataGovSgOptions.SectionName).Get<DataGovSgOptions>()!;
            http.BaseAddress = new Uri(options.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddStandardResilienceHandler(o =>
        {
            o.Retry.MaxRetryAttempts = 2;
            o.Retry.Delay = TimeSpan.FromSeconds(5);
            o.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            o.Retry.UseJitter = true;
            o.Retry.ShouldHandle = args => ValueTask.FromResult(
                args.Outcome.Result is { StatusCode: not System.Net.HttpStatusCode.TooManyRequests }
                    and ({ StatusCode: System.Net.HttpStatusCode.RequestTimeout }
                         or { StatusCode: >= System.Net.HttpStatusCode.InternalServerError })
                || args.Outcome.Exception is HttpRequestException);

            o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
            o.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(2);
            o.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(1);
        });

        services.AddKeyedTransient<IDatasetParser>(ParserKeys.Infections,
            (_, _) => new LongFormatParser(DatasetSpecs.Infections));
        services.AddKeyedTransient<IDatasetParser>(ParserKeys.Admissions,
            (_, _) => new LongFormatParser(DatasetSpecs.Admissions));
        services.AddKeyedTransient<IDatasetParser>(ParserKeys.AverageCases,
            (_, _) => new LongFormatParser(DatasetSpecs.AverageCases));
        services.AddKeyedTransient<IDatasetParser>(ParserKeys.IcuBeds,
            (_, _) => new LongFormatParser(DatasetSpecs.IcuBeds));

        services.AddScoped<IObservationRepository, ObservationRepository>();
        services.AddScoped<IDatasetCatalogue, DatasetCatalogue>();
        services.AddScoped<IDatasetSyncService, DatasetSyncService>();
        services.AddScoped<DevDataSeeder>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}