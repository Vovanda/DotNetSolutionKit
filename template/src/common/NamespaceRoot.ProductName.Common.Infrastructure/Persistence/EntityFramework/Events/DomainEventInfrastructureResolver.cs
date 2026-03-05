using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NamespaceRoot.ProductName.Common.Application.Events;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Events;

/// <summary>
/// Provides a unified way to resolve scoped domain event infrastructure 
/// from singleton interceptors within DbContextPool.
/// </summary>
internal static class DomainEventInfrastructureResolver
{
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public static bool TryResolve(
        DbContext context, 
        out IDomainEventStorage storage, 
        out IDomainEventDispatcher dispatcher)
    {
        storage = null!;
        dispatcher = null!;

        // 1. Get the internal EF service provider safely.
        var internalSp = (context as IInfrastructure<IServiceProvider>)?.Instance;
        if (internalSp is null) return false;

        // 2. Get the EF Diagnostics Logger to provide visibility for DI issues (addressing review comments).
        var diagLogger = internalSp.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>();

        // 3. Identify the Root Application Provider.
        var appSp = internalSp.GetService<IDbContextOptions>()
            ?.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault()
            ?.ApplicationServiceProvider;

        // 4. Determine the best available Scope.
        var httpAccessor = (appSp ?? internalSp).GetService<IHttpContextAccessor>();
        var contextSp = internalSp.GetService<IServiceProvider>();

        // GUARD: Prevent InvalidOperationException when trying to resolve Scoped services from Root.
        // This commonly happens during Seeding or Migrations if no manual Scope is created.
        if (contextSp == appSp && httpAccessor?.HttpContext is null)
        {
            diagLogger?.Logger.LogDebug("DomainEvent resolution skipped: Root Provider detected without an active Scope.");
            return false;
        }

        // 5. Try providers in priority order until both services are found.
        IServiceProvider?[] candidates =
        [
            httpAccessor?.HttpContext?.RequestServices,
            contextSp,
            appSp
        ];

        foreach (var candidate in candidates)
        {
            if (candidate is null) continue;
            storage = candidate.GetService<IDomainEventStorage>()!;
            dispatcher = candidate.GetService<IDomainEventDispatcher>()!;
            if (storage is not null && dispatcher is not null)
                return true;
        }

        diagLogger?.Logger.LogWarning(
            "DomainEvent infrastructure (Storage/Dispatcher) not found in the current scope for {Context}.",
            context.GetType().Name);
        storage = null!;
        dispatcher = null!;

        return true;
    }
}