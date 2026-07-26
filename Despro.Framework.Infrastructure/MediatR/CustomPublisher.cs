using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.Infrastructure.MediatR;

public class CustomPublisher : ICustomPublisher
{
    private readonly IServiceProvider _serviceFactory;

    public CustomPublisher(IServiceProvider serviceFactory)
    {
        _serviceFactory = serviceFactory;

        _publishStrategies[PublishStrategy.Async] = new CustomMediator(_serviceFactory, AsyncContinueOnException);
        _publishStrategies[PublishStrategy.ParallelNoWait] = new CustomMediator(_serviceFactory, ParallelNoWait);
        _publishStrategies[PublishStrategy.ParallelWhenAll] = new CustomMediator(_serviceFactory, ParallelWhenAll);
        _publishStrategies[PublishStrategy.ParallelWhenAny] = new CustomMediator(_serviceFactory, ParallelWhenAny);
        _publishStrategies[PublishStrategy.SyncContinueOnException] = new CustomMediator(_serviceFactory, SyncContinueOnException);
        _publishStrategies[PublishStrategy.SyncStopOnException] = new CustomMediator(_serviceFactory, SyncStopOnException);
    }

    private readonly IDictionary<PublishStrategy, IMediator> _publishStrategies = new Dictionary<PublishStrategy, IMediator>();
    private PublishStrategy DefaultStrategy { get; set; } = PublishStrategy.SyncContinueOnException;

    public Task Publish<TNotification>(TNotification notification)
        => Publish(notification, DefaultStrategy, CancellationToken.None);

    public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy)
        => Publish(notification, strategy, CancellationToken.None);

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
        => Publish(notification, DefaultStrategy, cancellationToken);

    public async Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken)
    {
        if (!_publishStrategies.TryGetValue(strategy, out var mediator))
        {
            throw new ArgumentException($"Unknown strategy: {strategy}");
        }

        await mediator.Publish(notification, cancellationToken);
    }


    #region Parallel
    private Task ParallelWhenAll(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        var tasks = handlers.Select(handler => Task.Run(async () =>
        {
            using var scope = _serviceFactory.CreateScope();

            await ExecuteHandlerInScope(scope.ServiceProvider, handler, notification, cancellationToken);
        }, cancellationToken)).ToList();

        return Task.WhenAll(tasks);
    }

    private Task ParallelWhenAny(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        var tasks = handlers.Select(handler => Task.Run(async () =>
        {
            using var scope = _serviceFactory.CreateScope();

            await ExecuteHandlerInScope(scope.ServiceProvider, handler, notification, cancellationToken);
        }, cancellationToken)).ToList();

        return Task.WhenAny(tasks);
    }

    private Task ParallelNoWait(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            Task.Run(async () =>
            {
                using var scope = _serviceFactory.CreateScope();

                await ExecuteHandlerInScope(scope.ServiceProvider, handler, notification, cancellationToken);
            }, cancellationToken);
        }

        return Task.CompletedTask;
    }

    private static async Task AsyncContinueOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
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

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }

    private static async Task SyncStopOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task SyncContinueOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
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

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }

    private async Task ExecuteHandlerInScope(IServiceProvider scopedProvider,
        NotificationHandlerExecutor executor,
        INotification notification,
        CancellationToken cancellationToken)
    {
        var handlerType = executor.HandlerInstance.GetType();

        var scopedHandler = scopedProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod("Handle");

        if (method != null)
        {
            var task = (Task)method.Invoke(scopedHandler, [notification, cancellationToken])!;
            await task;
        }
    }
    #endregion
}