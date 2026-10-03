using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NamespaceRoot.ProductName.Common.Infrastructure.Security;
using NamespaceRoot.ProductName.Common.Web.Errors;
using NamespaceRoot.ProductName.Common.Web.Health;
using NamespaceRoot.ProductName.Common.Web.Swagger;

namespace NamespaceRoot.ProductName.Common.Web.Setup;

/// <summary>
/// The web layer every service host shares: what is registered and the order of the pipeline.
/// </summary>
/// <remarks>
/// Kept here rather than in each service, so a fix to the pipeline reaches every service with a
/// Common update instead of a change copied into each of them. A service adds only what is its own:
/// its configuration, its registrations, its health checks and its authentication handler.
/// </remarks>
public static class PlatformWebHost
{
    /// <summary>
    /// Registers JSON, error handling, controllers, validation, Swagger, CORS and the execution context.
    /// </summary>
    /// <param name="builder">The service's builder.</param>
    /// <param name="serviceAssembly">The service's API assembly: its validators, controllers' versions
    /// and XML comments are read from it.</param>
    /// <param name="configureMvc">The service's own MVC additions: filters, application parts.</param>
    public static WebApplicationBuilder AddPlatformWebApi(
        this WebApplicationBuilder builder,
        Assembly serviceAssembly,
        Action<IMvcBuilder>? configureMvc = null)
    {
        builder.Services.ConfigurePlatformJson();

        // Errors are RFC 9457 problems with a correlation identifier; see Common.Web/Errors.
        builder.Services.AddPlatformErrorHandling(builder.Environment);

        var mvc = builder.Services.AddControllers();
        configureMvc?.Invoke(mvc);

        builder.Services.AddValidation(serviceAssembly);
        builder.SetupSwaggerPage(serviceAssembly);
        builder.AddPlatformCors();

        builder.Services.AddMemoryCache();
        builder.Services.AddExecutionContext();
        builder.Services.AddSingleton(TimeProvider.System);

        return builder;
    }

    /// <summary>
    /// Builds the pipeline in the order the platform relies on.
    /// </summary>
    /// <param name="app">The built application.</param>
    /// <param name="serviceAssembly">The service's API assembly, for the Swagger page.</param>
    /// <param name="authenticate">Whether to add authentication and authorization; false only where
    /// the schemes were not registered, as in a schema-only run.</param>
    /// <param name="beforeEndpoints">The service's own middleware, after authorization and before
    /// the endpoints: a dashboard, for instance.</param>
    /// <remarks>
    /// Needs <see cref="PlatformLogging.AddPlatformLogging"/> on the builder: the request log line comes
    /// from Serilog. Tracing goes first, so the request log line carries the correlation identifier. Error handling
    /// goes after CORS, so an error reaches a browser with CORS headers, and before authentication, so
    /// failures there are answered as problems too.
    /// </remarks>
    public static WebApplication UsePlatformPipeline(
        this WebApplication app,
        Assembly serviceAssembly,
        bool authenticate = true,
        Action<WebApplication>? beforeEndpoints = null)
    {
        app.UsePlatformTracing();
        app.UsePlatformRequestLogging();
        app.UseRouting();
        app.UsePlatformCors();
        app.UsePlatformErrorHandling();

        if (authenticate)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseSwaggerPage(serviceAssembly);
        beforeEndpoints?.Invoke(app);

        app.MapControllers();
        app.MapPlatformHealth();

        return app;
    }

    /// <summary>
    /// camelCase and no nulls, for controllers and for anything written with <c>WriteAsJsonAsync</c>.
    /// </summary>
    public static IServiceCollection ConfigurePlatformJson(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => Apply(options.JsonSerializerOptions));
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => Apply(options.SerializerOptions));
        return services;

        static void Apply(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }
    }
}
