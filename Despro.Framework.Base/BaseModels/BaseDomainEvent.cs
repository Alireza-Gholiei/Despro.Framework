using MediatR;

namespace Despro.Framework.Base.BaseModels;

public abstract class BaseDomainEvent : INotification
{
    public long? EventCreateDate { get; protected set; } = DateTime.UtcNow.Ticks;
}