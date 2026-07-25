namespace MohCovidInsights.Application.Abstractions.Interfaces;

public interface IQueryDispatcher
{
    Task<Result<TResult>> SendAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}
