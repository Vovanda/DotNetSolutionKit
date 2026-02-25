using NamespaceRoot.ProductName.Common.Domain;
using NamespaceRoot.ProductName.Common.Domain.Events;
using IAggregateRoot = Antwerp.EsimPlatform.Common.Domain.IAggregateRoot;

namespace NamespaceRoot.ProductName.Common;

/// <summary>
/// Base class for business entities that act as Aggregate Roots and support domain events.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Explicit implementation to hide infrastructure cleanup from the public API.
    /// </summary>
    void IHasDomainEvents.ClearDomainEvents() => _domainEvents.Clear();
}