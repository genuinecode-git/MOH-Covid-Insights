using System.Text.Json;
using MohCovidInsights.Infrastructure.Parsing;

namespace MohCovidInsights.Infrastructure.DataGovSg;

/// <summary>
/// Anti-corruption layer over the data.gov.sg CKAN datastore API. One request per
/// dataset (paginated only if a dataset exceeds the page size), returning typed JSON
/// with declared field metadata — as opposed to the two-step CSV poll-download flow.
/// </summary>
public sealed class DataGovSgClient(HttpClient http, IOptions<DataGovSgOptions> options, ILogger<DataGovSgClient> logger) : IDataGovSgClient
{
    private readonly DataGovSgOptions _options = options.Value;

    public async Task<DatasetPayload> DownloadAsync(string datasetId, CancellationToken ct = default)
    {
        var pages = new List<string>();
        var offset = 0;
        var total = int.MaxValue;

        for (var page = 0; page < _options.MaxPages && offset < total; page++)
        {
            var path = $"api/action/datastore_search" +
                       $"?resource_id={Uri.EscapeDataString(datasetId)}" +
                       $"&limit={_options.PageSize}&offset={offset}";

            using var response = await http.GetAsync(path, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(30);
                logger.LogWarning("Rate limited on {DatasetId}, waiting {Seconds}s",
                    datasetId, wait.TotalSeconds);
                await Task.Delay(wait, ct);
                continue;
            }

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(ct);

            var (success, count, reportedTotal) = Inspect(body, datasetId);
            if (!success)
                throw new DataGovSgException($"data.gov.sg reported failure for {datasetId}.");

            pages.Add(body);
            total = reportedTotal;
            offset += count;

            logger.LogDebug("Dataset {DatasetId}: fetched {Count} of {Total} records",datasetId, offset, total);

            if (count == 0) break;
        }

        if (pages.Count == 0)
            throw new DataGovSgException($"No data returned for {datasetId}.");

        return new DatasetPayload(string.Concat(pages), JsonTable.Load(pages));
    }

    private static (bool Success, int Count, int Total) Inspect(string body, string datasetId)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        var success = !root.TryGetProperty("success", out var s) || s.GetBoolean();

        if (!root.TryGetProperty("result", out var result))
            throw new DataGovSgException($"Malformed response for {datasetId}: no 'result'.");

        var count = result.TryGetProperty("records", out var records)
            ? records.GetArrayLength()
            : 0;

        var total = result.TryGetProperty("total", out var t) && t.TryGetInt32(out var parsed)
            ? parsed
            : count;

        return (success, count, total);
    }
}

public sealed class DataGovSgException(string message) : Exception(message);