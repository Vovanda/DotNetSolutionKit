

using NamespaceRoot.ProductName.Common.Infrastructure.Configuration;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class CorsSetup
{
    public static WebApplicationBuilder SetupCors(this WebApplicationBuilder builder)
    {
        var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>(); 

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ServiceNameOrCustomCors", policy =>
            {
                policy.SetIsOriginAllowed(origin => IsOriginMatch(origin, corsSettings?.AllowedOrigins));

                if (corsSettings?.AllowedMethods.Length > 0)
                    policy.WithMethods(corsSettings.AllowedMethods);
                else
                    policy.AllowAnyMethod();

                if (corsSettings?.AllowedHeaders.Length > 0)
                    policy.WithHeaders(corsSettings.AllowedHeaders);
                else
                    policy.AllowAnyHeader();

                if (corsSettings?.AllowCredentials == true)
                    policy.AllowCredentials();

                if (corsSettings?.ExposedHeaders.Length > 0)
                    policy.WithExposedHeaders(corsSettings.ExposedHeaders);

                if (corsSettings?.PreflightMaxAge > 0)
                    policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.PreflightMaxAge));
            });
        });

        return builder;
    }

    public static WebApplication UseAppCors(this WebApplication app)
    {
        app.UseCors("ServiceNameOrCustomCors");
        return app;
    }

    private static bool IsOriginMatch(string origin, string[]? allowedPatterns)
    {
        if (string.IsNullOrEmpty(origin) || allowedPatterns == null) return false;

        foreach (var pattern in allowedPatterns)
        {
            if (pattern == "*") return true;

            if (pattern.EndsWith(":*"))
            {
                var basePrefix = pattern.Substring(0, pattern.Length - 1); // "https://localhost:"
                if (origin.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (string.Equals(origin, pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}