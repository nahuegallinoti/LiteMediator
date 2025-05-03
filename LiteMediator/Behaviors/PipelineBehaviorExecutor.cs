using LiteMediator.Abstractions;

namespace LiteMediator.Behaviors;

public static class PipelineBehaviorExecutor
{
    public static ValueTask<TResponse> ExecutePipeline<TRequest, TResponse>(
    TRequest request,
    IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors,
    RequestExecutionDelegate<TResponse> handler,
    CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        if (behaviors == null || !behaviors.Any())
            return handler();

        RequestExecutionDelegate<TResponse> current = handler;

        foreach (var behavior in behaviors.Reverse())
        {
            var next = current;
            current = () => behavior.Handle(request, cancellationToken, next);
        }

        return current();
    }
}