using MediatR;

namespace Despro.Framework.Infrastructure.MediatR;

public class CustomMediator(
    IServiceProvider serviceFactory,
    Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish)
    : Mediator(serviceFactory)
{
    protected override Task PublishCore(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        => publish(handlerExecutors, notification, cancellationToken);
}