using NamespaceRoot.ProductName.Common.Domain.Context;
using NamespaceRoot.ProductName.Common.Infrastructure.Security;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Events;

/// <summary>
/// Carries the operator who enqueued a job into the job itself.
/// </summary>
/// <remarks>
/// <para>
/// Internal endpoints trigger jobs on demand: an administrator closes a cycle, replays a delivery,
/// re-syncs a provider. The work then happens on a Hangfire thread with no HTTP context, and
/// everything it writes would be attributed to the system - "the platform closed this cycle", with
/// no way to tell it from the nightly run that does the same thing unattended.
/// </para>
/// <para>
/// Identity is captured at enqueue time, when the claims are still there, and restored around the
/// execution. Scheduled jobs carry no such parameter and stay system-attributed, which is the
/// truthful answer for them.
/// </para>
/// </remarks>
public sealed class JobActorPropagationFilter(IServiceProvider services)
    : IClientFilter, IServerFilter
{
    private const string UserIdParameter = "ActorUserId";
    private const string LoginParameter = "ActorLogin";
    private const string TenantParameter = "ActorTenantId";

    public void OnCreating(CreatingContext context)
    {
        // Resolved per enqueue rather than injected: the filter is a singleton and the actor is not.
        using var scope = services.CreateScope();
        var actor = scope.ServiceProvider.GetService<IUserContext>();

        if (actor is null || IsAnonymous(actor)) return;

        context.SetJobParameter(UserIdParameter, actor.UserId);
        context.SetJobParameter(LoginParameter, actor.Login);
        context.SetJobParameter(TenantParameter, actor.TenantId);
    }

    public void OnCreated(CreatedContext context) { }

    public void OnPerforming(PerformingContext context)
    {
        var userId = context.GetJobParameter<Guid?>(UserIdParameter);
        if (userId is not { } id || id == Guid.Empty) return;

        var actor = new JobTriggeredByUserContext(
            id,
            context.GetJobParameter<string?>(LoginParameter),
            context.GetJobParameter<Guid?>(TenantParameter));

        context.Items[nameof(JobActorContext)] = JobActorContext.Use(actor);
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Items.TryGetValue(nameof(JobActorContext), out var scope) && scope is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>
    /// Whether there is nobody to carry - a scheduled run, or an enqueue from another job.
    /// </summary>
    private static bool IsAnonymous(IUserContext actor)
    {
        try
        {
            return actor.IsSystemCall || actor.UserId == Guid.Empty;
        }
        catch
        {
            // No claims to read at all: enqueued from a job or at startup.
            return true;
        }
    }
}
