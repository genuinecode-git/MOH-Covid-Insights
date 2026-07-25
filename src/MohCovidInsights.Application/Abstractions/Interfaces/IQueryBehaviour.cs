namespace MohCovidInsights.Application.Abstractions.Interfaces;

public interface IQueryBehaviour<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(
        TQuery query,
        Func<Task<Result<TResult>>> next,
        CancellationToken ct);
}
