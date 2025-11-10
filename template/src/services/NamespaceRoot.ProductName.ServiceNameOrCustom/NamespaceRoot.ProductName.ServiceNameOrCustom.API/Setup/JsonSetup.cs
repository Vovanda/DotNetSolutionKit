using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

/// <summary>
/// Centralized JSON serialization configuration
/// </summary>
internal static class JsonSetup
{
    /// <summary>
    /// Common serializer options used across the entire application
    /// </summary>
    public static readonly JsonSerializerOptions CommonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Configures JSON options for both MVC Controllers and Minimal API/HttpContext extensions
    /// </summary>
    /// <param name="builder">Web application builder</param>
    /// <returns>Web application builder</returns>
    public static WebApplicationBuilder SetupJson(this WebApplicationBuilder builder)
    {
        // Configure MVC Controllers JSON options
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = CommonOptions.PropertyNamingPolicy;
            options.JsonSerializerOptions.DefaultIgnoreCondition = CommonOptions.DefaultIgnoreCondition;
        });

        // Configure HttpContext.Response.WriteAsJsonAsync options (Minimal API core)
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = CommonOptions.PropertyNamingPolicy;
            options.SerializerOptions.DefaultIgnoreCondition = CommonOptions.DefaultIgnoreCondition;
        });

        return builder;
    }
}
