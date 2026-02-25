using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Events;

/// <summary>
/// Finalizes the domain event lifecycle by executing Post-Commit or Rollback phases.
/// </summary>
/// <remarks>
/// <b>Note:</b> Must be registered as a <b>Singleton</b> to support <b>DbContextPool</b>.
/// Infrastructure is resolved via <see cref="DomainEventInfrastructureResolver"/>.
/// </remarks>
public sealed class DomainEventTransactionInterceptor : DbTransactionInterceptor
{
    public override async Task TransactionCommittedAsync(
        DbTransaction transaction, 
        TransactionEndEventData eventData, 
        CancellationToken ct = default)
    {
        var context = eventData.Context;
        
        // Resolve infrastructure. Skip if in Root Provider or outside of a valid Scope.
        if (context is null || !DomainEventInfrastructureResolver.TryResolve(context, out var storage, out var dispatcher))
        {
            await base.TransactionCommittedAsync(transaction, eventData, ct);
            return;
        }

        var events = storage.GetEvents();
        if (events.Any())
        {
            try
            {
                // PHASE 2: Post-Commit (side effects after successful DB commit).
                await dispatcher.DispatchPostCommitAsync(events, ct);
            }
            finally
            {
                // Ensure storage is cleared even if dispatch fails to prevent event duplication.
                storage.Clear();
            }
        }

        await base.TransactionCommittedAsync(transaction, eventData, ct);
    }

    public override async Task TransactionRolledBackAsync(
        DbTransaction transaction, 
        TransactionEndEventData eventData, 
        CancellationToken ct = default)
    {
        var context = eventData.Context;

        if (context is null || !DomainEventInfrastructureResolver.TryResolve(context, out var storage, out var dispatcher))
        {
            await base.TransactionRolledBackAsync(transaction, eventData, ct);
            return;
        }

        var events = storage.GetEvents();
        if (events.Any())
        {
            try
            {
                // PHASE 3: Rollback (notifying subscribers about failure with the captured exception).
                await dispatcher.DispatchRollbackAsync(events, storage.LastException, ct);
            }
            finally
            {
                storage.Clear();
            }
        }

        await base.TransactionRolledBackAsync(transaction, eventData, ct);
    }
}
