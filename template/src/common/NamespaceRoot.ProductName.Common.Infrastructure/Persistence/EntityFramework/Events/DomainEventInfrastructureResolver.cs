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

        // 4. Determine the best available scope.
        // HTTP requests: IHttpContextAccessor.HttpContext.RequestServices
        // MassTransit/Hangfire: DomainEventScopeContext set by DomainEventScopeFilter
        // Seeding/Migrations: no scope → returns false, events are silently skipped.
        var httpAccessor = appSp?.GetService<IHttpContextAccessor>();

        // 5. Try providers in priority order until both services are found.
        IServiceProvider?[] candidates =
        [
            httpAccessor?.HttpContext?.RequestServices,
            DomainEventScopeContext.Current,
        ];

        foreach (var candidate in candidates)
        {
            if (candidate is null) continue;
            storage = candidate.GetService<IDomainEventStorage>()!;
            dispatcher = candidate.GetService<IDomainEventDispatcher>()!;
            if (storage is not null && dispatcher is not null)
                return true;
        }

        diagLogger?.Logger.LogDebug(
            "DomainEvent resolution skipped: no active scope found for {Context}. Expected in background workers (Outbox delivery, Hangfire).",
            context.GetType().Name);
        storage = null!;
        dispatcher = null!;

        return false;
    }
}