using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NamespaceRoot.ProductName.Common.Domain.Persistence;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Repositories.EntityFramework;

public abstract class DbContextBase : DbContext, IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;
    
    protected DbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    #region IUnitOfWork Implementation

    // ReSharper disable once RedundantOverriddenMember
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    public Dictionary<string, (Type Type, object? OriginalValue, object? CurrentValue)> GetChangesFor(
        object entity, bool isNewEntity = false)
    {
        var trackedEntry = ChangeTracker.Entries().FirstOrDefault(x => x.Entity == entity);
        
        if (trackedEntry == null)
        {
            throw new ArgumentException("Entity is not being tracked by this context", nameof(entity));
        }

        var result = new Dictionary<string, (Type, object?, object?)>();

        foreach (var property in trackedEntry.Properties.OrderBy(p => p.Metadata.Name))
        {
            if (property.Metadata.IsShadowProperty())
                continue;

            var original = property.OriginalValue;
            var current = property.CurrentValue;

            if (isNewEntity)
            {
                if (current is null ||
                    current is false ||
                    (current is string s && string.IsNullOrEmpty(s)) ||
                    (current is DateTimeOffset dto && dto == default))
                {
                    continue;
                }
                result[property.Metadata.Name] = (property.Metadata.ClrType, null, current);
            }
            else
            {
                if (!Equals(original, current))
                {
                    result[property.Metadata.Name] = (property.Metadata.ClrType, original, current);
                }
            }
        }

        return result;
    }

    #endregion

    #region Transaction Management

    /// <summary>
    /// Starts a new transaction.
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction to commit");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// Gets the current transaction if exists.
    /// </summary>
    public IDbContextTransaction? GetCurrentTransaction() => _currentTransaction;
    #endregion

    #region Helper Methods

    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    public int GetPendingChangesCount() => ChangeTracker.Entries()
        .Count(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

    public void DiscardChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }
    }

    #endregion

    #region Dispose Management

    public new void Dispose()
    {
        DisposeTransaction();
        base.Dispose();
    }
    
    public new async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        await base.DisposeAsync();
    }

    private void DisposeTransaction()
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    #endregion
}