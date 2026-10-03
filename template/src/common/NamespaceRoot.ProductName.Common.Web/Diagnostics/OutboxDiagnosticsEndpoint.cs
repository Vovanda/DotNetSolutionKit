using NamespaceRoot.ProductName.Common.Infrastructure.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NamespaceRoot.ProductName.Common.Web.Diagnostics;

/// <summary>
/// Minimal API endpoint that exposes the MassTransit EF Outbox state for live debugging.
/// Gated on environment: registered only outside Production (Local / Development / Staging),
/// so a real prod deployment never carries the endpoint at all — no auth check to forget,
/// no token to compromise. Each service wires its own <c>DbContext</c> + schema-qualified
/// outbox table name (e.g. <c>auth.outbox_message</c>).
/// </summary>
public static class OutboxDiagnosticsEndpoint
{
    private const string Route = "/api/v1/diagnostics/outbox-stats";

    public static WebApplication MapOutboxDiagnostics<TDbContext>(this WebApplication app, string outboxTable)
        where TDbContext : DbContext
    {
        if (app.Environment.IsProduction()) return app;

        app.MapGet(Route, async (
                HttpContext ctx,
                TDbContext db,
                string? filter,
                int? sampleSize,
                CancellationToken ct) =>
            {
                var response = await OutboxStatsQuery.RunAsync(db, outboxTable, filter, sampleSize ?? 5, ct);
                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithTags("Diagnostics")
            .WithSummary("Outbox state snapshot (non-production only).")
            .WithDescription(
                "Lets operators check whether outbound MassTransit messages are persisted by " +
                "the SaveChanges interceptor and drained by the BusOutboxDeliveryService worker. " +
                "Optional ?filter=<substring> matches the message body (typically an entity id).");

        return app;
    }
}
