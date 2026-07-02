using MediatR;

namespace Despro.Framework.Base.BaseModels;

public abstract class BaseDomainEvent : INotification
{
    public long Id { get; set; }
    public long? CreateDate { get; protected set; } = DateTime.UtcNow.Ticks;
    public long? CreateUserId { get; protected set; }
    public long? DeleteDate { get; protected set; }
    public long? DeleteUserId { get; protected set; }
}