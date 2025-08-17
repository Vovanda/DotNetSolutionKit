using Microsoft.OpenApi.Models;
using System.Reflection;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

/// <summary>
/// Настройка Swagger
/// </summary>
internal static class Swagger
{
    private const string SwaggerVersion = "v1";

    /// <summary>
    /// Настройка странички Swagger при сборке приложения
    /// </summary>
    public static WebApplicationBuilder SetupSwaggerPage(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

            options.SwaggerDoc(SwaggerVersion, new OpenApiInfo
            {
                Title = $"{GetSwaggerDisplayName(builder)}: {environment}",
                Version = SwaggerVersion,
                Description = builder.Configuration.GetValue<string>("Application:Description") ?? string.Empty
            });

            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            var xmlPath = GetXmlCommentsPath();
            if (!string.IsNullOrEmpty(xmlPath))
                options.IncludeXmlComments(xmlPath, true);

            options.DescribeAllParametersInCamelCase();
            options.SupportNonNullableReferenceTypes();
        });

        return builder;
    }

    /// <summary>
    /// Подключение Swagger UI в приложении
    /// </summary>
    public static WebApplication UseSwaggerPage(this WebApplication app)
    {
        if (app.Environment.IsProduction()) return app;

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            var displayName = GetSwaggerDisplayName(app);
            c.DocumentTitle = displayName;
            c.SwaggerEndpoint($"/swagger/{SwaggerVersion}/swagger.json", $"{displayName} {SwaggerVersion}");
        });

        return app;
    }

    private static string GetSwaggerDisplayName(WebApplicationBuilder builder) =>
        builder.Configuration.GetValue<string>("Application:Name") ?? string.Empty;

    private static string GetSwaggerDisplayName(WebApplication app) =>
        app.Configuration.GetValue<string>("Application:Name") ?? string.Empty;

    private static string? GetXmlCommentsPath()
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        return File.Exists(xmlPath) ? xmlPath : null;
    }
}