using System.ComponentModel.DataAnnotations.Schema;

namespace Despro.Framework.Base.BaseModels;

public abstract class AggregateRoot : Aggregate
{
    [NotMapped]
    public List<IDomainEvent> DomainEvents { get; } = [];

    protected void AddDomainEvent(IDomainEvent eventItem)
    {
        DomainEvents.Add(eventItem);
    }

    protected void RemoveDomainEvent(IDomainEvent eventItem)
    {
        DomainEvents?.Remove(eventItem);
    }
}