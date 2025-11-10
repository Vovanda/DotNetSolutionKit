using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;

public class MigrationRunner
{
    public static void RunMigrations(DbContext context, ILogger logger)
    {
        logger.LogInformation("Checking database schema state before applying migrations...");

        try
        {
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Found {Count} pending migrations: {Migrations}. Applying...",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                context.Database.Migrate();

                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date. No migrations to apply.");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Migration process failed! Root cause: {Message}", ex.Message);
            throw;
        }
    }
}