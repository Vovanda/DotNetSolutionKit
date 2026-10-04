using System.Diagnostics.CodeAnalysis;
//#if (Hangfire)
using Hangfire;
//#if (Database == "mssql")
using Hangfire.SqlServer;
//#else
using Hangfire.PostgreSql;
//#endif
//#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NamespaceRoot.ProductName.Common.Application.Configuration;
using NamespaceRoot.ProductName.Common.Application.Persistence;
using NamespaceRoot.ProductName.Common.Domain.Persistence;
using NamespaceRoot.ProductName.Common.Domain.Specifications;
using NamespaceRoot.ProductName.Common.Infrastructure.Configuration;
//#if (Messaging != "none")
using NamespaceRoot.ProductName.Common.Infrastructure.Messaging;
//#endif
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework;
//#if (AuditEnabled)
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Audit;
//#endif
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Events;
//#if (Database == "mssql")
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.SqlServer;
//#else
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.Postgres;
//#endif
//#if (ClickHouse)
using NamespaceRoot.ProductName.Common.Infrastructure.ClickHouse;
//#endif
//#if (Storage)
using NamespaceRoot.ProductName.Common.Infrastructure.Storage;
//#endif
using NamespaceRoot.ProductName.ServiceNameOrCustom.Application;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework.DataSeeding;
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework.Specifications;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure;

/// <summary>
/// Extensions for registering infrastructure services in DI container.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
public static class DependencyInjection
{
    /// <summary>
    /// Register infrastructure services.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // What this run uses: Database, HangfireSettings and RabbitMq each have an Enabled switch.
        var switches = DependencySwitches.Read(configuration);

        // Database
        string connectionString;
        if (switches.Database)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. Set ConnectionStrings__DefaultConnection, " +
                    $"or run without a database: {DependencySwitches.DatabaseKey}=false.");

            // Unique name for this microservice (used for schema ownership)
            var serviceName = typeof(DomainMarker).Namespace!;

            // 1. Guard for Main Database Schema
//#if (Database == "mssql")
            SqlServerSchemaGuard.EnsureExclusiveSchema(connectionString, ServiceIdentifierDbContext.DefaultSchemaName, serviceName);
//#else
            PostgresSchemaGuard.EnsureExclusiveSchema(connectionString, ServiceIdentifierDbContext.DefaultSchemaName, serviceName);
//#endif
        }
        else
        {
            // The context stays registered so everything built on it still resolves; opening a
            // connection answers 503 instead.
            connectionString = SwitchedOffDatabase.ConnectionString;
        }

        // Domain events: handlers from the application layer, run in three phases around SaveChanges and
        // the transaction by the interceptors attached to the context below.
        services.AddDomainEvents(typeof(ApplicationMarker).Assembly);

        // AddDbContext, not the pool: interceptors resolved per scope (the domain events', the outbox's)
        // are not re-attached to a context handed back by the pool, and they would quietly do nothing.
        services.AddDbContext<ServiceIdentifierDbContext>((sp, options) =>
        {
//#if (Database == "mssql")
            options.UseSqlServer(connectionString,
                x => { x.MigrationsHistoryTable("__EFMigrationsHistory", ServiceIdentifierDbContext.DefaultSchemaName); });
//#else
            options.UseNpgsql(connectionString,
                x => { x.MigrationsHistoryTable("__EFMigrationsHistory", ServiceIdentifierDbContext.DefaultSchemaName); });
//#endif
            if (!switches.Database)
                options.UseSwitchedOffDatabase();
            options.ApplyDomainEventInterceptors(sp);
//#if (AuditEnabled)
            options.ApplyAuditInterceptor(sp);
//#endif
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ServiceIdentifierDbContext>());

//#if (Messaging == "outbox")
        // The bus, with the outbox in this service's schema: a message and the change that caused it
        // commit together. Consumers are found in this assembly.
        services.AddMessaging<ServiceIdentifierDbContext>(
            configuration, "servicenameorcustom", typeof(InfrastructureMarker).Assembly);
//#if (AuditEnabled)

        // The audit journal: a change to an entity marked [Auditable] publishes AuditRecordedV1 into the
        // outbox in the same save, so a rolled back change leaves no entry. IAuditRecorder and
        // ISetBasedAuditCapture record what bypasses the change tracker; see docs/features/audit.md.
        services.AddAuditPersistence();
        services.AddAuditRecorder<ServiceIdentifierDbContext>("servicenameorcustom");
//#endif

//#elif (Messaging == "direct")
        // The bus without an outbox: a message goes straight to RabbitMQ and is lost if the broker is
        // down. Consumers are found in this assembly.
        services.AddMessaging(configuration, "servicenameorcustom", typeof(InfrastructureMarker).Assembly);

//#endif
        // Repositories
        
        // Specifications
//#if (Database == "mssql")
        services.AddScoped<ICaseInsensitiveSearch, SqlServerCaseInsensitiveSearch>();
//#else
        services.AddScoped<ICaseInsensitiveSearch, PostgresCaseInsensitiveSearch>();
//#endif

        // Readable numbers from a sequence, taken before the entity is created
//#if (Database == "mssql")
        services.AddScoped<IShortIdGenerator, SqlServerShortIdGenerator<ServiceIdentifierDbContext>>();
//#else
        services.AddScoped<IShortIdGenerator, PostgresShortIdGenerator<ServiceIdentifierDbContext>>();
//#endif
        
//#if (ClickHouse)
        // ClickHouse, the ClickHouse section: connections, the schema check, readiness
        services.AddClickHouse(configuration);

//#endif
//#if (Storage)
        // Object storage, the S3 section; S3:Enabled=false keeps nothing
        services.AddS3ObjectStorage(configuration);

//#endif
        // Configurations
        services.AddInfrastructureConfiguration();
        
//#if (Hangfire)
        // Background jobs, with their settings validated only when the job server runs. Off together
        // with the database: the database is their storage.
        if (switches.Jobs)
        {
            services.AddValidatedOptions<IHangfireSettings, HangfireSettings>(HangfireSettings.SectionName);
            services.AddBackgroundJobs(connectionString);
        }

//#endif
        // Data Seeding
        services.AddScoped<DataSeeder>();

        return services;
    }

//#if (Hangfire)
//#if (Database == "mssql")
    // "Hangfire" in ASCII: one key for every service of the database.
    private const long HangfireInstallLockKey = 0x48616E6766697265;

//#endif
    /// <summary>
    /// Register Hangfire and background job services.
    /// </summary>
    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, string connectionString)
    {
        var serviceName = typeof(DomainMarker).Namespace!;
        var hangfireSchemaName = $"{ServiceIdentifierDbContext.DefaultSchemaName}_hangfire";

        // Guard for Hangfire Schema
//#if (Database == "mssql")
        SqlServerSchemaGuard.EnsureExclusiveSchema(connectionString, hangfireSchemaName, serviceName);

        // Hangfire.SqlServer installs its tables in a transaction that deadlocks with another service
        // installing into the same database at the same moment, and gives up after three attempts, leaving
        // the service without a job server. The installs take turns under one lock for the whole database.
        var installLock = new SqlServerMigrationLock();
        using (var connection = installLock.Connect(connectionString))
        {
            installLock.Acquire(connection, HangfireInstallLockKey);
            SqlServerObjectsInstaller.Install(connection, hangfireSchemaName);
            installLock.Release(connection, HangfireInstallLockKey);
        }
//#else
        PostgresSchemaGuard.EnsureExclusiveSchema(connectionString, hangfireSchemaName, serviceName);
//#endif

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
//#if (Database == "mssql")
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                SchemaName = hangfireSchemaName,
                // Installed above, under the lock.
                PrepareSchemaIfNecessary = false
            }));
//#else
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
            {
                SchemaName = hangfireSchemaName,
                PrepareSchemaIfNecessary = true
            }));
//#endif
    
        services.AddHangfireServer((sp, options) =>
        {
            var settings = sp.GetRequiredService<IHangfireSettings>();
            options.WorkerCount = settings.WorkerCount;
        });

        // Jobs run in a scope the domain event interceptors can see, so events a job raises are not
        // dropped; the filter carries the person who enqueued a job into it, for attribution.
        services.AddDomainEventJobActivator();
        services.AddHostedService(sp => new HangfireFilterInstaller(sp));

        return services;
    }
//#endif

    /// <summary>
    /// Register and validate infrastructure configuration settings.
    /// </summary>
    /// <param name="services">Service collection.</param>
    private static void AddInfrastructureConfiguration(this IServiceCollection services)
    {
        services.AddValidatedOptions<ICorsSettings, CorsSettings>(CorsSettings.SectionName);
    }
    
    /// <summary>
    /// Helper method to register validated options with interface.
    /// </summary>
    private static IServiceCollection AddValidatedOptions<TInterface, TSettings>(
        this IServiceCollection services, 
        string sectionName)
        where TInterface : class
        where TSettings : class, TInterface, new()
    {
        services.AddOptions<TSettings>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    
        services.AddSingleton<TInterface>(sp => 
            sp.GetRequiredService<IOptions<TSettings>>().Value);

        return services;
    }
}
