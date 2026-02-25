using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Events;

/// <summary>
/// Interceptor that captures domain events and executes the Pre-Save phase.
/// </summary>
/// <remarks>
/// <b>Note:</b> Must be registered as a <b>Singleton</b> to support <b>DbContextPool</b>.
/// Infrastructure is resolved via <see cref="DomainEventInfrastructureResolver"/>.
/// </remarks>
public sealed class DomainEventPreSaveInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var context = data.Context;

        // Guard: Skip if no context or no pending changes.
        if (context is null || !context.ChangeTracker.HasChanges())
            return await base.SavingChangesAsync(data, result, ct);

        // Identify entities implementing IHasDomainEvents that have pending events.
        // We use the interface to support explicit implementation and decoupling.
        var entries = context.ChangeTracker.Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        if (entries.Count == 0)
            return await base.SavingChangesAsync(data, result, ct);

        // Resolve infrastructure only when there is actual work to do.
        if (!DomainEventInfrastructureResolver.TryResolve(context, out var storage, out var dispatcher) || storage.IsDispatching)
            return await base.SavingChangesAsync(data, result, ct);

        try
        {
            storage.IsDispatching = true;
            
            // Extract events before clearing them from entities.
            var events = entries.SelectMany(e => e.Entity.DomainEvents).ToList();

            // 1. Store events in scoped storage FIRST. 
            // This ensures they are available for TransactionRolledBackAsync if Pre-Save fails.
            storage.AddEvents(events);

            // 2. Clear events from entities to prevent duplicate dispatching during subsequent saves.
            // Works via explicit interface implementation to maintain domain encapsulation.
            entries.ForEach(e => e.Entity.ClearDomainEvents());

            // 3. PHASE 1: Pre-Save execution (validations, state adjustments within the transaction).
            await dispatcher.DispatchPreSaveAsync(events, ct);
        }
        catch (Exception ex)
        {
            // Capture exception for the Rollback phase to provide diagnostic context.
            storage.LastException = ex; 
            throw;
        }
        finally
        {
            storage.IsDispatching = false;
        }

        return await base.SavingChangesAsync(data, result, ct);
    }
}
