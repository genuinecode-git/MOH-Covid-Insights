using Amazon.Lambda.Core;

using MohCovidInsights.Infrastructure;
using MohCovidInsights.Infrastructure.Persistence;
using MohCovidInsights.Infrastructure.Sync;
using MohCovidInsights.Infrastructure.Sync.Interfaces;

namespace MohCovidInsights.Ingestion;

public sealed class Function : IAsyncDisposable
{
    private readonly ServiceProvider _services;

    public Function()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .AddRdsSecretIfPresent()
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b
            .AddLambdaLogger()
            .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information));
        services.AddInfrastructure(configuration);

        _services = services.BuildServiceProvider();
    }

    /// <summary>Invoked by EventBridge. Throws on any dataset failure so Lambda's error metric fires.</summary>
    public async Task<SyncSummary> HandleAsync(object? input, ILambdaContext? context = null)
    {
        await using var scope = _services.CreateAsyncScope();
        var sync = scope.ServiceProvider.GetRequiredService<IDatasetSyncService>();

        var summary = await sync.SyncAllAsync();

        var message = $"Sync complete: {summary.Succeeded} succeeded, " +
                      $"{summary.Skipped} skipped, {summary.Failed} failed, {summary.TotalRows} rows";

        context?.Logger.LogInformation(message);
        Console.WriteLine(message);

        return summary.Failed > 0 ? throw new InvalidOperationException($"{summary.Failed} dataset(s) failed to ingest.") : summary;
    }

    public ValueTask DisposeAsync() => _services.DisposeAsync();
}