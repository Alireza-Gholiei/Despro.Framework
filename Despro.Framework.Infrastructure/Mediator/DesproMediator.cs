using Despro.Framework.Base.IMediator;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Despro.Framework.Infrastructure.Mediator;

public sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapper> Wrappers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerWrapper<TResponse>)Wrappers.GetOrAdd(
            request.GetType(),
            static requestType =>
            {
                var responseType = requestType
                    .GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                    .GetGenericArguments()[0];

                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
                return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            });

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }
}

internal abstract class RequestHandlerWrapper;

internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerWrapper
{
    public abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        var behaviors = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse();

        RequestHandlerDelegate<TResponse> pipeline = ct => handler.Handle((TRequest)request, ct);

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = ct => behavior.Handle((TRequest)request, next, ct);
        }

        return pipeline(cancellationToken);
    }
}

/*************************************/
public sealed record NotificationHandlerExecutor(Type HandlerType,
    Func<INotification, CancellationToken, Task> HandlerCallback,
    Func<IServiceProvider, INotification, CancellationToken, Task> ScopedCallback);

/*************************************/
internal abstract class NotificationHandlerWrapper
{
    public abstract IReadOnlyList<NotificationHandlerExecutor> GetHandlers(IServiceProvider serviceProvider);
}

internal sealed class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override IReadOnlyList<NotificationHandlerExecutor> GetHandlers(IServiceProvider serviceProvider)
    {
        return serviceProvider
            .GetServices<INotificationHandler<TNotification>>()
            .Select(handler => new NotificationHandlerExecutor(
                handler.GetType(),
                (n, ct) => handler.Handle((TNotification)n, ct),
                (sp, n, ct) =>
                {
                    var scopedHandler = (INotificationHandler<TNotification>)sp.GetRequiredService(handler.GetType());
                    return scopedHandler.Handle((TNotification)n, ct);
                }))
            .ToList();
    }
}