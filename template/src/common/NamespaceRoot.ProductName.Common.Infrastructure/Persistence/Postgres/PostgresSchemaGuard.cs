using Npgsql;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Persistence.Postgres;

/// <summary>
/// Provides PostgreSQL-specific safety checks for database schema isolation across microservices.
/// </summary>
public static class PostgresSchemaGuard
{
    /// <summary>
    /// Ensures that the specified PostgreSQL schema is either empty or owned by the current service.
    /// This prevents accidental schema sharing between different microservices.
    /// </summary>
    /// <param name="connectionString">PostgreSQL database connection string.</param>
    /// <param name="schemaName">The schema name to validate (e.g., 'auth' or 'auth_hangfire').</param>
    /// <param name="serviceName">Unique identity of the current service (e.g., assembly name).</param>
    /// <exception cref="InvalidOperationException">Thrown when the schema is already occupied by another service.</exception>
    public static void EnsureExclusiveSchema(string connectionString, string schemaName, string serviceName)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var transaction = conn.BeginTransaction();
        try
        {
            // 1. Ensure the target schema exists
            using (var cmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\";", conn, transaction))
            {
                cmd.ExecuteNonQuery();
            }

            // 2. Create a metadata table to persist schema ownership if it doesn't exist
            var createTableSql = $@"
                CREATE TABLE IF NOT EXISTS ""{schemaName}"".""_service_metadata"" (
                    key TEXT PRIMARY KEY,
                    value TEXT
                );";
            using (var cmd = new NpgsqlCommand(createTableSql, conn, transaction))
            {
                cmd.ExecuteNonQuery();
            }

            // 3. Verify ownership identity with a row-level lock (FOR UPDATE)
            string? currentOwner;
            using (var cmd = new NpgsqlCommand(
                       $@"SELECT value FROM ""{schemaName}"".""_service_metadata"" WHERE key = 'owner' FOR UPDATE", 
                       conn, transaction))
            {
                currentOwner = cmd.ExecuteScalar() as string;
            }

            if (currentOwner == null)
            {
                // Schema is unowned - claim it for this service
                using var insertCmd = new NpgsqlCommand(
                    $@"INSERT INTO ""{schemaName}"".""_service_metadata"" (key, value) VALUES ('owner', @name)", 
                    conn, transaction);
                insertCmd.Parameters.AddWithValue("name", serviceName);
                insertCmd.ExecuteNonQuery();
            }
            else if (!string.Equals(currentOwner, serviceName, StringComparison.OrdinalIgnoreCase))
            {
                // Conflict detected - another service is already using this schema
                throw new InvalidOperationException(
                    $"CRITICAL: PostgreSQL schema '{schemaName}' is already owned by '{currentOwner}'. " +
                    $"Service '{serviceName}' is not allowed to use it to avoid data contamination.");
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
