using NamespaceRoot.ProductName.Common.Web.Misc;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

/// <summary>
/// Configures global error handling and status code mapping for the application pipeline.
/// </summary>
internal static class ErrorHandlingSetup
{
    /// <summary>
    /// Adds global error handling and status code mapping to the pipeline.
    /// Must be called early in the middleware chain (after CORS, before Auth).
    /// </summary>
    /// <remarks>
    /// NOTE: This middleware handles 500 (via ExceptionHandler) and empty 404/405 responses.
    /// Automatic 400 Bad Request (ModelState validation) is NOT handled here because 
    /// it is triggered by the MVC layer earlier. 
    /// To customize 400 errors, use 'ConfigureApiBehaviorOptions' in 'SetupWebApi'.
    /// </remarks>
    public static WebApplication UseAppErrorHandling(this WebApplication app)
    {
        // 1. Handles unhandled exceptions (500, 422, 403-from-filters etc. via GlobalExceptionHandler)
        app.UseExceptionHandler();

        // 2. Handles "empty" responses where status was set but nobody was written (e.g. Routing 404/405)
        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            
            // Skip if response has already started or content is present 
            // (prevents overwriting 400/401/403 responses already formed by Auth or MVC)
            if (response.HasStarted || response.ContentLength.HasValue || !string.IsNullOrEmpty(response.ContentType))
                return;

            var error = response.StatusCode switch
            {
                StatusCodes.Status404NotFound => 
                    ErrorResponseHelper.NotFound("The requested resource was not found."),
                
                StatusCodes.Status405MethodNotAllowed => 
                    ErrorResponseHelper.CreateCustom(StatusCodes.Status405MethodNotAllowed, "HTTP method not allowed for this endpoint."),
                
                _ => null
            };

            if (error != null)
            {
                response.ContentType = "application/json";
                // Use centralized JsonSetup.CommonOptions for consistent naming policy
                await response.WriteAsJsonAsync(error, JsonSetup.CommonOptions);
            }
        });

        return app;
    }
}
