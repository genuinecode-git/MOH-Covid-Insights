
namespace MohCovidInsights.Infrastructure.DataGovSg.Interfaces;

/// <summary>Raw response retained for hashing; table is the parsed view.</summary>
public sealed record DatasetPayload(string Raw, ITabularData Table);

public interface IDataGovSgClient
{
    Task<DatasetPayload> DownloadAsync(string datasetId, CancellationToken ct = default);
}