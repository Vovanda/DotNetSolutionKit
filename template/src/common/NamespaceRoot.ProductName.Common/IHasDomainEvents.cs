using NamespaceRoot.ProductName.Common.Domain.Events;

namespace NamespaceRoot.ProductName.Common;

/// <summary>
/// Defines the contract for entities that support a domain event lifecycle.
/// Intended primarily for infrastructure layers (Interceptors, Dispatchers) 
/// to manage event flow without polluting the public business API of the entity.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events raised by the entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all captured domain events from the entity.
    /// Usually called by the persistence layer after events have been successfully 
    /// extracted for pre-save or post-commit processing.
    /// </summary>
    void ClearDomainEvents();
}