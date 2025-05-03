namespace LiteMediator.Behaviors;

public interface IPipelineBehavior<TRequest, TResponse>
{
    ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestExecutionDelegate<TResponse> next);
}