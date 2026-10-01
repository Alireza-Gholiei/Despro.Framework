using Despro.Framework.Base.IMediator;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Despro.Framework.Infrastructure.Mediator;

public class CustomPublisher(IServiceProvider serviceFactory) : ICustomPublisher
{
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> Wrappers = new();

    private delegate Task PublishStrategyDelegate(
        IReadOnlyList<NotificationHandlerExecutor> handlers,
        INotification notification,
        CancellationToken cancellationToken);

    private PublishStrategy DefaultStrategy { get; set; } = PublishStrategy.SyncContinueOnException;

    public Task Publish<TNotification>(TNotification notification) where TNotification : INotification
        => Publish(notification, DefaultStrategy, CancellationToken.None);

    public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy) where TNotification : INotification
        => Publish(notification, strategy, CancellationToken.None);

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken) where TNotification : INotification
        => Publish(notification, DefaultStrategy, cancellationToken);

    public async Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        PublishStrategyDelegate publish = strategy switch
        {
            PublishStrategy.Async => AsyncContinueOnException,
            PublishStrategy.ParallelNoWait => ParallelNoWait,
            PublishStrategy.ParallelWhenAll => ParallelWhenAll,
            PublishStrategy.ParallelWhenAny => ParallelWhenAny,
            PublishStrategy.SyncContinueOnException => SyncContinueOnException,
            PublishStrategy.SyncStopOnException => SyncStopOnException,
            _ => throw new ArgumentException($"Unknown strategy: {strategy}")
        };

        // resolve با نوع runtime (مثل رفتار Publish(object) در MediatR)
        var wrapper = Wrappers.GetOrAdd(notification.GetType(), static type =>
            (NotificationHandlerWrapper)Activator.CreateInstance(
                typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(type))!);

        var handlers = wrapper.GetHandlers(serviceFactory);

        if (handlers.Count == 0)
            return;

        await publish(handlers, notification, cancellationToken);
    }

    #region Strategies

    private Task ParallelWhenAll(IReadOnlyList<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        var tasks = handlers
            .Select(handler => Task.Run(() => ExecuteInNewScope(handler, notification, cancellationToken), cancellationToken))
            .ToList();

        return Task.WhenAll(tasks);
    }

    private Task ParallelWhenAny(IReadOnlyList<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        var tasks = handlers
            .Select(handler => Task.Run(() => ExecuteInNewScope(handler, notification, cancellationToken), cancellationToken))
            .ToList();

        return Task.WhenAny(tasks);
    }

    private Task ParallelNoWait(IReadOnlyList<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ExecuteInNewScope(handler, notification, cancellationToken), cancellationToken);
        }

        return Task.CompletedTask;
    }

    private static async Task AsyncContinueOnException(IReadOnlyList<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        List<Task> tasks = [];
        List<Exception> exceptions = [];

        foreach (var handler in handlers)
        {
            try
            {
                tasks.Add(handler.HandlerCallback(notification, cancellationToken));
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                exceptions.Add(ex);
            }
        }

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (AggregateException ex)
        {
            exceptions.AddRange(ex.Flatten().InnerExceptions);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            exceptions.Add(ex);
        }

        if (exceptions.Count != 0)
            throw new AggregateException(exceptions);
    }

    private static async Task SyncStopOnException(IReadOnlyList<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task SyncContinueOnException(IReadOnlyList<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        List<Exception> exceptions = [];

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                exceptions.AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count != 0)
            throw new AggregateException(exceptions);
    }

    private async Task ExecuteInNewScope(NotificationHandlerExecutor executor, INotification notification, CancellationToken cancellationToken)
    {
        using var scope = serviceFactory.CreateScope();
        await executor.ScopedCallback(scope.ServiceProvider, notification, cancellationToken);
    }

    #endregion
}