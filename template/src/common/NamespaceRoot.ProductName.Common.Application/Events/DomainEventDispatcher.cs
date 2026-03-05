using Microsoft.Extensions.DependencyInjection;
using NamespaceRoot.ProductName.Common.Application.Events.Handlers;
using NamespaceRoot.ProductName.Common.Domain.Events;

namespace NamespaceRoot.ProductName.Common.Application.Events;

/// <summary>
/// Orchestrates the execution of domain event handlers across different transaction phases.
/// Resolves handlers from the DI container and ensures proper data passing.
/// </summary>
public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    /// <inheritdoc />
    public Task DispatchPreSaveAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
        => DispatchAsync(typeof(IDomainPreSaveHandler<>), events, ct);

    /// <inheritdoc />
    public Task DispatchPostCommitAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
        => DispatchAsync(typeof(IDomainPostCommitHandler<>), events, ct);

    /// <inheritdoc />
    public Task DispatchRollbackAsync(IEnumerable<IDomainEvent> events, Exception? exception, CancellationToken ct = default)
        => DispatchAsync(typeof(IDomainRollbackHandler<>), events, ct, exception);

    // TODO: Known limitation — a class implementing both IDomainPreSaveHandler<T> and IDomainPostCommitHandler<T>
    //       for the same event type will be resolved and called in BOTH phases (same Handle method twice).
    //       DI registers it under both interfaces via .AsImplementedInterfaces().
    //       Fix options:
    //       a) Enforce single-phase per class at registration time (scan + guard).
    //       b) Deduplicate resolved instances per dispatch call by checking implemented interfaces.
    //       Until fixed: one class = one phase only.

    /// <summary>
    /// Dispatches events to handlers matching the specific phase interface and event type.
    /// </summary>
    private async Task DispatchAsync(Type openHandlerType, IEnumerable<IDomainEvent> events, CancellationToken ct, object? data = null)
    {
        foreach (var @event in events)
        {
            // Resolve concrete phase interface (e.g., IDomainPostCommitHandler<UserCreatedEvent>)
            var handlerType = openHandlerType.MakeGenericType(@event.GetType());
            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                // Bridge call via the non-generic IDomainEventHandler interface
                if (handler is IDomainEventHandler baseHandler)
                {
                    try
                    {
                        await baseHandler.Handle(@event, ct, data);
                    }
                    catch (Exception)
                    {
                        // Critical: Re-throw only during Pre-Save to allow transaction rollback.
                        // Post-Commit and Rollback errors should be logged but not interrupt the flow.
                        if (openHandlerType == typeof(IDomainPreSaveHandler<>)) throw;
                    }
                }
            }
        }
    }
}