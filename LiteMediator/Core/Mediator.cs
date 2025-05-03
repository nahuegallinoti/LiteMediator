using LiteMediator.Abstractions;
using LiteMediator.Behaviors;
using LiteMediator.Exceptions;
using System.Collections.Concurrent;
using System.Reflection;

namespace LiteMediator.Core;

public class Mediator(ServiceFactory serviceFactory) : IMediator
{
    private readonly ServiceFactory _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));

    private readonly ConcurrentDictionary<Type, object> _requestHandlers = [];
    private readonly ConcurrentDictionary<Type, object> _notificationHandlers = [];
    private readonly ConcurrentDictionary<Type, object> _streamHandlers = [];
    private readonly ConcurrentDictionary<Type, object> _pipelineBehaviors = [];

    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = request.GetType();

        if (!typeof(IRequest<TResponse>).IsAssignableFrom(requestType))
            throw new ArgumentException($"El tipo de request no es compatible con IRequest<{typeof(TResponse).Name}>");

        // Obtener los handlers
        var handler = _requestHandlers.GetOrAdd(
            requestType,
            static (type, factory) =>
            {
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(type, typeof(TResponse));
                return factory(handlerType)!;
            },
            _serviceFactory) ?? throw new HandlerNotFoundException(requestType);

        // Obtener los behaviors del pipeline
        var behaviors = _pipelineBehaviors.GetOrAdd(
            requestType,
            static (type, factory) =>
            {
                var behaviorType = typeof(IEnumerable<>).MakeGenericType(typeof(IPipelineBehavior<,>).MakeGenericType(type, typeof(TResponse)));
                return factory(behaviorType) ?? Enumerable.Empty<IPipelineBehavior<IRequest<TResponse>, TResponse>>();
            },
            _serviceFactory);

        return InvokePipelineDynamic<TResponse>(request, behaviors, handler, cancellationToken);
    }


    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(nameof(notification));

        var handlers = (IEnumerable<INotificationHandler<TNotification>>)_notificationHandlers.GetOrAdd(
            typeof(TNotification),
            static (notificationType, factory) =>
            {
                var handlerType = typeof(IEnumerable<>).MakeGenericType(typeof(INotificationHandler<>).MakeGenericType(notificationType));
                return factory(handlerType) ?? Array.Empty<INotificationHandler<TNotification>>();
            },
            _serviceFactory
        );

        var tasks = handlers.Select(h => h.Handle(notification, cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nameof(notification));

        Type notificationType = notification.GetType();

        Type handlerType = typeof(IEnumerable<>).MakeGenericType(typeof(INotificationHandler<>).MakeGenericType(notificationType));
        var handlers = (IEnumerable<object>?)_serviceFactory(handlerType) ?? [];

        var tasks = handlers.Select(handler =>
        {
            var method = handler.GetType().GetMethod(nameof(INotificationHandler<INotification>.Handle))!;
            return (Task)method.Invoke(handler, [notification, cancellationToken])!;
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = request.GetType();

        var handler = _streamHandlers.GetOrAdd(
            requestType,
            static (type, factory) =>
            {
                var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(type, typeof(TResponse));
                return factory(handlerType) ?? throw new HandlerNotFoundException(type);
            },
            _serviceFactory);

        MethodInfo method = handler.GetType().GetMethod("Handle")!;
        return (IAsyncEnumerable<TResponse>)method.Invoke(handler, [request, cancellationToken])!;
    }

    private static ValueTask<TResponse> InvokePipeline<TRequest, TResponse>(TRequest request,
                                                                    IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors,
                                                                    IRequestHandler<TRequest, TResponse> handler,
                                                                    CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        return PipelineBehaviorExecutor.ExecutePipeline(
            request,
            behaviors,
            () => handler.Handle(request, cancellationToken),
            cancellationToken);
    }

    private ValueTask<TResponse> InvokePipelineDynamic<TResponse>(object request, object behaviors,
                                                                  object handler, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();

        // Obtener el método InvokePipeline de la clase Mediator
        MethodInfo method = typeof(Mediator)
            .GetMethod(nameof(InvokePipeline), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)!
            .MakeGenericMethod(requestType, typeof(TResponse));

        // Invocar el método con los parámetros necesarios
        var result = (ValueTask<TResponse>)method.Invoke(this, [request, behaviors, handler, cancellationToken])!;

        return result;
    }
}

public delegate object? ServiceFactory(Type serviceType);