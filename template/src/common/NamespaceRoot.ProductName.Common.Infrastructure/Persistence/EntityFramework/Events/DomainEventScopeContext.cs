namespace NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Events;

/// <summary>
/// Ambient scope holder for domain event infrastructure resolution in non-HTTP contexts.
/// Uses <see cref="AsyncLocal{T}"/> to propagate the current DI scope across async call chains
/// (e.g., MassTransit consumers, Hangfire jobs).
/// </summary>
internal static class DomainEventScopeContext
{
    private static readonly AsyncLocal<IServiceProvider?> _current = new();

    /// <summary>Gets the current ambient scope's service provider, or null if not set.</summary>
    public static IServiceProvider? Current => _current.Value;

    /// <summary>
    /// Sets the ambient scope to the given <paramref name="serviceProvider"/> for the duration of the returned scope.
    /// Restores the previous value on dispose.
    /// </summary>
    public static IDisposable Use(IServiceProvider serviceProvider)
    {
        var previous = _current.Value;
        _current.Value = serviceProvider;
        return new RestoreScope(() => _current.Value = previous);
    }

    private sealed class RestoreScope(Action restore) : IDisposable
    {
        public void Dispose() => restore();
    }
}
