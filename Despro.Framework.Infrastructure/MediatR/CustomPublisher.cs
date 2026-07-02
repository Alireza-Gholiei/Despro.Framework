using MediatR;

namespace Despro.Framework.Infrastructure.MediatR;

public class CustomPublisher : ICustomPublisher
{
    public CustomPublisher(IServiceProvider serviceFactory)
    {
        var serviceFactory1 = serviceFactory;

        PublishStrategies[PublishStrategy.Async] = new CustomMediator(serviceFactory1, AsyncContinueOnException);
        PublishStrategies[PublishStrategy.ParallelNoWait] = new CustomMediator(serviceFactory1, ParallelNoWait);
        PublishStrategies[PublishStrategy.ParallelWhenAll] = new CustomMediator(serviceFactory1, ParallelWhenAll);
        PublishStrategies[PublishStrategy.ParallelWhenAny] = new CustomMediator(serviceFactory1, ParallelWhenAny);
        PublishStrategies[PublishStrategy.SyncContinueOnException] = new CustomMediator(serviceFactory1, SyncContinueOnException);
        PublishStrategies[PublishStrategy.SyncStopOnException] = new CustomMediator(serviceFactory1, SyncStopOnException);
    }

    public IDictionary<PublishStrategy, IMediator> PublishStrategies = new Dictionary<PublishStrategy, IMediator>();
    public PublishStrategy DefaultStrategy { get; set; } = PublishStrategy.SyncContinueOnException;

    public Task Publish<TNotification>(TNotification notification)
    {
        try
        {
            return Publish(notification, DefaultStrategy, CancellationToken.None);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy)
    {
        try
        {
            return Publish(notification, strategy, CancellationToken.None);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            return Publish(notification, DefaultStrategy, cancellationToken);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public async Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken)
    {
        try
        {
            if (!PublishStrategies.TryGetValue(strategy, out var mediator))
            {
                throw new ArgumentException($"Unknown strategy: {strategy}");
            }

            await mediator.Publish(notification, cancellationToken);
        }
        catch (Exception e)
        {
            throw e;
        }
    }


    #region Parallel
    private Task ParallelWhenAll(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        try
        {
            List<Task> tasks = new();

            foreach (var handler in handlers)
            {
                tasks.Add(Task.Run(() => handler.HandlerCallback(notification, cancellationToken), cancellationToken));
            }

            return Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    private Task ParallelWhenAny(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        try
        {
            List<Task> tasks = new();

            foreach (var handler in handlers)
            {
                tasks.Add(Task.Run(() => handler.HandlerCallback(notification, cancellationToken), cancellationToken));
            }

            return Task.WhenAny(tasks);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    private Task ParallelNoWait(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var handler in handlers)
            {
                Task.Run(() => handler.HandlerCallback(notification, cancellationToken), cancellationToken);
            }

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    private async Task AsyncContinueOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        try
        {
            List<Task> tasks = new();
            List<Exception> exceptions = new();

            foreach (var handler in handlers)
            {
                try
                {
                    tasks.Add(handler.HandlerCallback(notification, cancellationToken));
                }
                catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
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
            catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
            {
                exceptions.Add(ex);
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    private async Task SyncStopOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var handler in handlers)
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    private async Task SyncContinueOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        try
        {
            List<Exception> exceptions = new();

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
                catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion
}