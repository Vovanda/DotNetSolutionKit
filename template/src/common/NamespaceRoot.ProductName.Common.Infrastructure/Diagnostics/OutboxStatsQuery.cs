using NamespaceRoot.ProductName.Common.Contracts.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Diagnostics;

/// <summary>
/// Reads the MassTransit EF Outbox table for any service that wires
/// <c>AddEntityFrameworkOutbox</c>. The table name is passed by the caller because each
/// service owns its own outbox table in a per-service schema (auth.outbox_message,
/// orders.outbox_message, notifications.outbox_message). The caller supplies a compile-time
/// constant — never user input — so the raw-SQL string interpolation here is safe.
/// </summary>
public static class OutboxStatsQuery
{
    public static async Task<OutboxStatsResponse> RunAsync(
        DbContext db,
        string outboxTable,
        string? filter,
        int sampleSize,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(outboxTable))
            throw new ArgumentException("Outbox table must be specified.", nameof(outboxTable));

        // Aggregate counters across the whole table — pending vs sent + the age of the
        // oldest pending row + the freshness of the last sent row tells the operator
        // at a glance whether the delivery worker is keeping up or stopped draining.
        // MassTransit's EF migration makes "SentTime" NOT NULL and writes the .NET
        // DateTime.MinValue sentinel (0001-01-01) for rows that haven't been delivered
        // yet, so a "pending" row is one with "SentTime" at or before 1900-01-01.
        var totals = await db.Database
            .SqlQueryRaw<OutboxTotals>($@"
                SELECT
                    COUNT(*) FILTER (WHERE ""SentTime"" <= TIMESTAMPTZ '1900-01-01')::bigint AS ""Pending"",
                    COUNT(*) FILTER (WHERE ""SentTime"" >  TIMESTAMPTZ '1900-01-01')::bigint AS ""Sent"",
                    MIN(""EnqueueTime"") FILTER (WHERE ""SentTime"" <= TIMESTAMPTZ '1900-01-01') AS ""OldestPendingAt"",
                    MAX(""SentTime"")    FILTER (WHERE ""SentTime"" >  TIMESTAMPTZ '1900-01-01') AS ""LastSentAt""
                FROM {outboxTable}")
            .SingleAsync(ct);

        // Sample of the most recent rows for hands-on inspection; respects the optional
        // body-substring filter so the caller can zoom in on one publish (e.g. an order id).
        var sql = $@"
            SELECT ""MessageId""::text AS ""MessageId"",
                   ""EnqueueTime""     AS ""EnqueueTime"",
                   CASE WHEN ""SentTime"" <= TIMESTAMPTZ '1900-01-01' THEN NULL ELSE ""SentTime"" END AS ""SentTime""
            FROM {outboxTable}
            WHERE (@filter IS NULL OR ""Body"" LIKE '%' || @filter || '%')
            ORDER BY ""EnqueueTime"" DESC
            LIMIT @sampleSize";

        // Explicit NpgsqlDbType — without it the provider can't infer the type of @filter
        // when the value is DBNull, raising `42P08: could not determine data type`.
        var filterParam = new NpgsqlParameter("filter", NpgsqlTypes.NpgsqlDbType.Text)
        {
            Value = (object?)filter ?? DBNull.Value
        };
        var sizeParam = new NpgsqlParameter("sampleSize", NpgsqlTypes.NpgsqlDbType.Integer)
        {
            Value = Math.Clamp(sampleSize, 1, 50)
        };

        var rows = await db.Database
            .SqlQueryRaw<OutboxRowSnapshot>(sql, filterParam, sizeParam)
            .ToListAsync(ct);

        return new OutboxStatsResponse
        {
            Pending = totals.Pending,
            Sent = totals.Sent,
            OldestPendingAt = totals.OldestPendingAt,
            LastSentAt = totals.LastSentAt,
            Sample = rows,
        };
    }

    private sealed record OutboxTotals(long Pending, long Sent, DateTime? OldestPendingAt, DateTime? LastSentAt);
}
