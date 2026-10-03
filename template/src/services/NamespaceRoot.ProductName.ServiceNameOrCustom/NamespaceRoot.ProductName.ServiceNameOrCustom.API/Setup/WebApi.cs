using Microsoft.AspNetCore.Mvc;
using NamespaceRoot.ProductName.Common.Infrastructure.Security;
using NamespaceRoot.ProductName.Common.Web.Errors;
using NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Interceptors;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

/// <summary>
/// Web API services and pipeline configuration
/// </summary>
internal static class WebApi
{
    /// <summary>
    /// Maps Web API controllers to the request pipeline
    /// </summary>
    public static WebApplication UseWebApi(this WebApplication app)
    {
        app.MapControllers();
    
        return app;
    }
    
    /// <summary>
    /// Configures Web API services, filters, and JSON serialization
    /// </summary>
    public static WebApplicationBuilder SetupWebApi(this WebApplicationBuilder builder)
    {
        // Apply centralized JSON configuration
        builder.SetupJson();
        // Errors are RFC 9457 problems with a correlation identifier; see Common.Web/Errors.
        builder.Services.AddPlatformErrorHandling(builder.Environment);
        
        var mvc = builder.Services.AddControllers(options =>
        {
            options.Filters.Add<PermissionAuthorizationFilter>();
        });
//#if (FeatureFlags)

        // The feature list endpoint ships with the platform, so every service answers about flags the
        // same way instead of each writing its own endpoint.
        mvc.AddApplicationPart(typeof(Common.Web.FeatureManagement.FeaturesController).Assembly);
//#endif
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddMemoryCache();
        builder.Services.AddExecutionContext();
        builder.Services.AddSingleton(TimeProvider.System);
        
        return builder;
    }
}