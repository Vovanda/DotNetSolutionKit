using System.Diagnostics;
using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers.Span;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Logging
{
    private static readonly string[] HealthCheckPaths = ["/health/ready", "/health/live", "/health/availability"];

    /// <summary>
    /// Настройка Serilog перед Build
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
            .Enrich.WithSpan()
            .MinimumLevel.Information()
            // Консоль
            .WriteTo.Console()
            // Файл
            //.WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
            // ELK (через HTTP или Serilog.Sinks.Elasticsearch)
            // .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["Elastic:Uri"]))
            // {
            //     AutoRegisterTemplate = true,
            //     IndexFormat = "myapp-{0:yyyy.MM.dd}"
            // })
            .Filter.ByExcluding(IsHealthCheckRequest)
            .ReadFrom.Configuration(configuration) // позволит переопределять через appsettings.json
            .CreateLogger();
        
        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Middleware логирования запросов с trace и лог старта/остановки приложения
    /// </summary>
    internal static WebApplication UseLogging(this WebApplication app)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? "App";

        // Логирование старта
        app.Logger.LogInformation("The {EntryAssemblyName} application started", assemblyName);

        // Логирование остановки
        app.Lifetime.ApplicationStopped.Register(() =>
        {
            app.Logger.LogInformation("The {EntryAssemblyName} application was stopped", assemblyName);
            Log.CloseAndFlush();
        });

        // Middleware для логирования запросов с TraceId
        app.Use(async (context, next) =>
        {
            using var activity = new Activity("HttpRequest");
            activity.Start();

            using (Serilog.Context.LogContext.PushProperty("TraceId", activity.TraceId))
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);

                await next.Invoke();

                logger.LogInformation("Finished request: {Method} {Path}", context.Request.Method, context.Request.Path);
            }
        });

        return app;
    }

    /// <summary>
    /// Фильтр health-check запросов для Serilog
    /// </summary>
    private static bool IsHealthCheckRequest(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("RequestPath", out var propertyValue))
        {
            var path = propertyValue.ToString().Trim('"');
            return HealthCheckPaths.Contains(path) && logEvent.Level <= LogEventLevel.Information;
        }
        return false;
    }
}
