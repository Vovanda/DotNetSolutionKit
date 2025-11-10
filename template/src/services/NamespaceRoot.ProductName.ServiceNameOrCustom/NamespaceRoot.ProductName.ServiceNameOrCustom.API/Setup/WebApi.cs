using Microsoft.AspNetCore.Mvc;
using NamespaceRoot.ProductName.Common.Infrastructure.Security;
using NamespaceRoot.ProductName.Common.Web.Misc;
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
        // Register the custom handler
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    
        // MANDATORY: Register ProblemDetails service to support UseExceptionHandler()
        builder.Services.AddProblemDetails();
        
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<PermissionAuthorizationFilter>();
        }).ConfigureApiBehaviorOptions(options =>
        {
            // This is the bridge between MVC Validation and your ErrorResponse contract
            options.InvalidModelStateResponseFactory = context =>
            {
                // We pass the naming policy from our central JsonSetup to avoid desync
                var errorResponse = ErrorResponseHelper.ValidationError(
                    context.ModelState, 
                    JsonSetup.CommonOptions.PropertyNamingPolicy);

                return new ObjectResult(errorResponse)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            };
        });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddMemoryCache();
        builder.Services.AddExecutionContext();
        builder.Services.AddSingleton(TimeProvider.System);
        
        return builder;
    }
}