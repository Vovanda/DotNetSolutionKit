using System.Diagnostics.CodeAnalysis;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NamespaceRoot.ProductName.Common.Domain.Persistence;
using NamespaceRoot.ProductName.Common.Domain.Specifications;
using NamespaceRoot.ProductName.Common.Infrastructure.Configuration;
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.Postgres;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework.DataSeeding;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework.Specifications;

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
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        // Unique name for this microservice (used for schema ownership)
        var serviceName = typeof(DomainMarker).Namespace!;

        // 1. Guard for Main Database Schema
        PostgresSchemaGuard.EnsureExclusiveSchema(connectionString, ServiceNameOrCustomDbContext.DefaultSchemaName, serviceName);

        services.AddDbContextPool<ServiceNameOrCustomDbContext>(options =>
                options.UseNpgsql(connectionString,
                    x => { x.MigrationsHistoryTable("__EFMigrationsHistory", ServiceNameOrCustomDbContext.DefaultSchemaName); }),
            poolSize: 1024
        );

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ServiceNameOrCustomDbContext>());

        // Repositories
        
        // Specifications
        services.AddScoped<ICaseInsensitiveSearch, PostgresCaseInsensitiveSearch>();
        
        // Configurations
        services.AddInfrastructureConfiguration();
        
        // Background jobs
        services.AddBackgroundJobs(connectionString);

        // Data Seeding
        services.AddScoped<DataSeeder>();
        
        // Polly Policies

        return services;
    }

    /// <summary>
    /// Register Hangfire and background job services.
    /// </summary>
    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, string connectionString)
    {
        var serviceName = typeof(DomainMarker).Namespace!;
        var hangfireSchemaName = $"{ServiceNameOrCustomDbContext.DefaultSchemaName}_hangfire";

        // Guard for Hangfire Schema
        PostgresSchemaGuard.EnsureExclusiveSchema(connectionString, hangfireSchemaName, serviceName);

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => 
                c.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
            {
                SchemaName = hangfireSchemaName,
                PrepareSchemaIfNecessary = true
            }));
    
        services.AddHangfireServer((sp, options) =>
        {
            var settings = sp.GetRequiredService<IHangfireSettings>();
            options.WorkerCount = settings.WorkerCount;
        });

        return services;
    }
    
    /// <summary>
    /// Register and validate infrastructure configuration settings.
    /// </summary>
    /// <param name="services">Service collection.</param>
    private static void AddInfrastructureConfiguration(this IServiceCollection services)
    {
        services
            .AddValidatedOptions<ICorsSettings, CorsSettings>(CorsSettings.SectionName)
            .AddValidatedOptions<IHangfireSettings, HangfireSettings>(HangfireSettings.SectionName);
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
