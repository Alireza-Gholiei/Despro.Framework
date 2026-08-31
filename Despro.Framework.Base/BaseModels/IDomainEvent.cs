using MediatR;

namespace Despro.Framework.Base.BaseModels;

public interface IDomainEvent : INotification
{
    long? EventCreateDate { get; }
}