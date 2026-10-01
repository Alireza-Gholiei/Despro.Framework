using System.ComponentModel.DataAnnotations.Schema;

namespace Despro.Framework.Base.BaseModels;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public abstract class AggregateRoot<TId> : Aggregate<TId>, IAggregateRoot where TId : notnull
{
    private readonly List<IDomainEvent> _events = [];
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events;

    public void ClearDomainEvents() => _events.Clear();
    protected void Raise(IDomainEvent e) => _events.Add(e);
    protected void RemoveDomainEvent(IDomainEvent eventItem) => _events.Remove(eventItem);
}