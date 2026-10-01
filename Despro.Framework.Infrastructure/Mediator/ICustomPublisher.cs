using Despro.Framework.Base.IMediator;

namespace Despro.Framework.Infrastructure.Mediator;

public interface ICustomPublisher
{
    Task Publish<TNotification>(TNotification notification) where TNotification : INotification;
    Task Publish<TNotification>(TNotification notification, PublishStrategy strategy) where TNotification : INotification;
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken) where TNotification : INotification;
    Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken) where TNotification : INotification;
}