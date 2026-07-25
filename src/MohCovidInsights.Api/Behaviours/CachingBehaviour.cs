using Microsoft.Extensions.Caching.Memory;

using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Domain.Common;

namespace MohCovidInsights.Api.Behaviours;

/// <summary>
/// The underlying datasets are archived, so identical queries are safe to cache
/// for long periods. Only successes are cached — failures must be retried.
/// </summary>
public sealed class CachingBehaviour<TQuery, TResult>(
    IMemoryCache cache,
    ILogger<CachingBehaviour<TQuery, TResult>> logger)
    : IQueryBehaviour<TQuery, TResult> where TQuery : IQuery<TResult>
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public async Task<Result<TResult>> HandleAsync(
        TQuery query, Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        var key = $"{typeof(TQuery).FullName}:{query}";

        if (cache.TryGetValue(key, out TResult? cached) && cached is not null)
        {
            logger.LogDebug("Cache hit for {Key}", key);
            return Result<TResult>.Success(cached);
        }

        var result = await next();
        if (result.IsSuccess)
            cache.Set(key, result.Value, Ttl);

        return result;
    }
}