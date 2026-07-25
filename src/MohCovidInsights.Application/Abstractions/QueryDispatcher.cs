using Microsoft.Extensions.DependencyInjection;

namespace MohCovidInsights.Application.Abstractions;

public sealed class QueryDispatcher(IServiceProvider services) : IQueryDispatcher
{
    public Task<Result<TResult>> SendAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = services.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for {query.GetType().Name}.");

        var behaviourType = typeof(IQueryBehaviour<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var behaviours = ((IEnumerable<object>)services.GetServices(behaviourType)).Reverse().ToList();

        var wrapper = (IDispatchWrapper<TResult>)Activator.CreateInstance(
            typeof(DispatchWrapper<,>).MakeGenericType(query.GetType(), typeof(TResult)),
            handler, behaviours)!;

        return wrapper.InvokeAsync(query, ct);
    }

    private interface IDispatchWrapper<TResult>
    {
        Task<Result<TResult>> InvokeAsync(object query, CancellationToken ct);
    }

    private sealed class DispatchWrapper<TQuery, TResult>(
        IQueryHandler<TQuery, TResult> handler,
        List<object> behaviours) : IDispatchWrapper<TResult>
        where TQuery : IQuery<TResult>
    {
        public Task<Result<TResult>> InvokeAsync(object query, CancellationToken ct)
        {
            var typed = (TQuery)query;
            Func<Task<Result<TResult>>> next = () => handler.HandleAsync(typed, ct);

            foreach (var behaviour in behaviours.Cast<IQueryBehaviour<TQuery, TResult>>())
            {
                var current = next;
                var captured = behaviour;
                next = () => captured.HandleAsync(typed, current, ct);
            }

            return next();
        }
    }
}