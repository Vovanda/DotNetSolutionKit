using System.Reflection;
using NamespaceRoot.ProductName.Common.Contracts.Health;
using Serilog;
using Serilog.Events;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Logging
{
    /// <summary>
    /// Configure Serilog before Build
    /// </summary>
    internal static WebApplicationBuilder SetupLogging(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("version", assembly.GetName().Version?.ToString() ?? "unknown")
            .Enrich.WithProperty("module", configuration["Application:Name"] ?? "App")
            .MinimumLevel.Information()
            .Filter.ByExcluding(IsHealthCheckRequest)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        
        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Request logging middleware with trace and app start/stop logging
    /// </summary>
    internal static WebApplication UseLogging(this WebApplication app)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? "App";

        // Application start logging
        app.Logger.LogInformation("The {EntryAssemblyName} application started", assemblyName);

        // Application stop logging
        app.Lifetime.ApplicationStopped.Register(() =>
        {
            app.Logger.LogInformation("The {EntryAssemblyName} application was stopped", assemblyName);
            Log.CloseAndFlush();
        });

        // Use built-in Serilog request logging instead of custom middleware
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (context, _, _) => 
                HealthConstants.AllPaths.Contains(context.Request.Path.Value) 
                    ? LogEventLevel.Verbose  // Health checks as verbose
                    : LogEventLevel.Information;
            
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
                {
                    diagnosticContext.Set("UserAgent", userAgent.ToString());
                }
            };
        });

        return app;
    }

    /// <summary>
    /// Health-check requests filter for Serilog
    /// </summary>
    private static bool IsHealthCheckRequest(LogEvent logEvent)
    {
        var isRequestPath = logEvent.Properties.TryGetValue("RequestPath", out var propertyValue);
        if (!isRequestPath) return false;
        var path = propertyValue!.ToString().Trim('"');
        return HealthConstants.AllPaths.Contains(path) && logEvent.Level <= LogEventLevel.Information;
    }
}