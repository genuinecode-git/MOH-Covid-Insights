namespace MohCovidInsights.Application.Abstractions.Interfaces;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct = default);
}
