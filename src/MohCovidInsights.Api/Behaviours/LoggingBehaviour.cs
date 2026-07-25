using System.Diagnostics;

using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Domain.Common;

namespace MohCovidInsights.Api.Behaviours;

public sealed class LoggingBehaviour<TQuery, TResult>(ILogger<LoggingBehaviour<TQuery, TResult>> logger)
    : IQueryBehaviour<TQuery, TResult> where TQuery : IQuery<TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TQuery query, Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        var name = typeof(TQuery).Name;
        var stopwatch = Stopwatch.StartNew();

        var result = await next();
        stopwatch.Stop();

        if (result.IsSuccess)
            logger.LogInformation("{Query} completed in {Elapsed}ms", name, stopwatch.ElapsedMilliseconds);
        else
            logger.LogWarning("{Query} failed with {Code}: {Message} ({Elapsed}ms)",
                name, result.Error.Code, result.Error.Message, stopwatch.ElapsedMilliseconds);

        return result;
    }
}