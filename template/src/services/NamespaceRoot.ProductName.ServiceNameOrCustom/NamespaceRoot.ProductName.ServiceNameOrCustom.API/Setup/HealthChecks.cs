using System.Reflection;
using NamespaceRoot.ProductName.Common.Contracts.Health;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class HealthChecks
{
    public static WebApplicationBuilder SetupHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        return builder;
    }

    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        var version = GetAssemblyVersion();
        var commit = GetGitCommit();
        
        app.MapGet(HealthConstants.Healthz, (TimeProvider timeProvider) => Results.Json(new
        {
            status = "Healthy",
            service = "ServiceNameOrCustom.API",
            version = version,
            commit = commit,
            timestamp = timeProvider.GetUtcNow()
        }));

        app.MapGet(HealthConstants.Readyz, async (TimeProvider timeProvider) => 
        {
            var isReady = await CheckDependencies(app);
            return isReady ? Results.Json(new
            {
                status = "Ready", 
                service = "ServiceNameOrCustom.API",
                version = version,
                commit = commit,
                timestamp = timeProvider.GetUtcNow()
            }) : Results.Json(new
            {
                status = "Unhealthy",
                service = "ServiceNameOrCustom.API",
                version = version,
                commit = commit,
                timestamp = timeProvider.GetUtcNow()
            }, statusCode: 503);
        });

        return app;
    }

    private static string GetAssemblyVersion()
    {
        return Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
    }

    /// <summary>
    /// The commit the service was built from: GIT_SHA when the deployment passes it (a Docker build
    /// has no .git folder), otherwise the GitCommit metadata Directory.Build.props records at build
    /// time, otherwise "local".
    /// </summary>
    private static string GetGitCommit()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("GIT_SHA");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        return Assembly.GetEntryAssembly()?
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "GitCommit")?
            .Value ?? "local";
    }

    private static async Task<bool> CheckDependencies(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ServiceNameOrCustomDbContext>();
            
            return await dbContext.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}