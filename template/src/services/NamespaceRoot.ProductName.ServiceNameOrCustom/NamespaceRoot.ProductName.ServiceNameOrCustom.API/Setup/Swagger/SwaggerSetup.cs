using System.Reflection;
using Microsoft.OpenApi.Models;
using NamespaceRoot.ProductName.Common.Infrastructure.Security;
using NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Swagger.Filters;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Swagger;

internal static class SwaggerSetup
{
    public static WebApplicationBuilder SetupSwaggerPage(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;
            
            var versions = ApiVersionHelper.DiscoverAllVersions();
            
            options.SwaggerDoc("all", new OpenApiInfo
            {
                Title = $"{builder.Configuration.GetValue<string>("Application:Name")} : {environment}",
                Version = "all",
                Description = builder.Configuration.GetValue<string>("Application:Description") ?? string.Empty
            });
            
            foreach (var version in versions)
            {
                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = $"{builder.Configuration.GetValue<string>("Application:Name")} : {environment}",
                    Version = version,
                    Description = builder.Configuration.GetValue<string>("Application:Description") ?? string.Empty
                });
            }

            // API Key authentication (X-API-Key header) - Always available
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Name = AuthHeaders.ApiKey,  // "X-API-Key"
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "API Key for authentication. Can be:\n" +
                            "1. User API Key (validates in database)\n" +
                            "2. Internal API Key (from configuration)"
            });

            // Check if JWT is configured
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var hasJwtConfig = jwtSection.Exists() && 
                              !string.IsNullOrEmpty(jwtSection["Issuer"]) && 
                              !string.IsNullOrEmpty(jwtSection["Audience"]);

            // JWT Bearer token (Authorization header) - Only if JWT is configured
            if (hasJwtConfig)
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = AuthHeaders.Authorization,  // "Authorization"
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Bearer token in format: 'Bearer {token}'."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        }, Array.Empty<string>()
                    }
                });
            }

            // API Key security requirement - Always added
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                    }, Array.Empty<string>()
                }
            });

            // Register operation filters
            options.OperationFilter<AuthorizationOperationFilter>();
            options.OperationFilter<PermissionsOperationFilter>();

            options.DocumentFilter<VersionedDocumentFilter>();

            // XML documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, true);

            options.DescribeAllParametersInCamelCase();
            options.SupportNonNullableReferenceTypes();
        });

        return builder;
    }

    public static WebApplication UseSwaggerPage(this WebApplication app)
    {
        if (app.Environment.IsProduction()) return app;

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            var displayName = app.Configuration.GetValue<string>("Application:ShortName") ?? string.Empty;
            c.DocumentTitle = displayName;
            
            c.SwaggerEndpoint($"/swagger/all/swagger.json", $"{displayName} - All");
            
            var versions = ApiVersionHelper.DiscoverAllVersions();
            foreach (var version in versions)
            {
                c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{displayName} - {version.ToUpper()}");
            }

            c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
            c.DisplayOperationId();
            c.EnableTryItOutByDefault();
        });

        return app;
    }
}