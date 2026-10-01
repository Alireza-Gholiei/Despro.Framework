using Despro.Framework.Base.IMediator;

namespace Despro.Framework.Base.BaseModels;

public interface IDomainEvent : INotification
{
    long? EventCreateDate { get; }
}