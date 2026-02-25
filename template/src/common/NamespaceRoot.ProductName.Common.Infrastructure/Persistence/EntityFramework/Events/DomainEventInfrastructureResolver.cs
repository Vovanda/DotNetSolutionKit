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

        var finalProvider = httpAccessor?.HttpContext?.RequestServices ?? contextSp ?? appSp;

        if (finalProvider is null) return false;

        // 5. Resolve infrastructure services.
        storage = finalProvider.GetService<IDomainEventStorage>()!;
        dispatcher = finalProvider.GetService<IDomainEventDispatcher>()!;

        // 6. Logging for missing registrations (ensures we don't "swallow" misconfigurations).
        if (storage is null || dispatcher is null)
        {
            diagLogger?.Logger.LogWarning(
                "DomainEvent infrastructure (Storage/Dispatcher) not found in the current scope for {Context}.", 
                context.GetType().Name);
            return false;
        }

        return true;
    }
}