using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NamespaceRoot.ProductName.Common.Application.Configuration;
using NamespaceRoot.ProductName.Common.Contracts.Responses;
using NamespaceRoot.ProductName.Common.Infrastructure.Security;
using NamespaceRoot.ProductName.Common.Web.Misc;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.Security;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.Security.Handlers;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Authentication
{
    public static WebApplication UseAppAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        return app;
    }
    
    public static WebApplicationBuilder SetupAppAuthentication(this WebApplicationBuilder builder)
    {
        // 1. Get configuration from appsettings
        var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>();
        var hasJwtConfig = jwtConfig != null && 
                          !string.IsNullOrEmpty(jwtConfig.Issuer) && 
                          !string.IsNullOrEmpty(jwtConfig.Audience);
        
        const string apiKeySchemeName = "ApiKey";
        
        // If JWT is configured, use combined scheme (JWT or API Key)
        if (hasJwtConfig)
        {
            SetupCombinedAuthentication(builder, jwtConfig!, apiKeySchemeName);
        }
        else
        {
            // If no JWT configuration, use only API Key authentication
            SetupApiKeyOnlyAuthentication(builder, apiKeySchemeName);
        }
        
        return builder;
    }

    private static void SetupCombinedAuthentication(
        WebApplicationBuilder builder, 
        IJwtConfiguration jwtConfig, 
        string apiKeySchemeName)
    {
        // RSA provider is needed for JWT signing key setup
        using var serviceProvider = builder.Services.BuildServiceProvider();
        var rsaKeyProvider = serviceProvider.GetRequiredService<IRsaKeyProvider>();
        var validationKey = rsaKeyProvider.ValidationKey;

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Jwt_Or_ApiKey";
            options.DefaultChallengeScheme = "Jwt_Or_ApiKey";
        })
        .AddPolicyScheme("Jwt_Or_ApiKey", "JWT or API Key", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
                    return JwtBearerDefaults.AuthenticationScheme;

                if (context.Request.Headers.ContainsKey(AuthHeaders.ApiKey))
                    return apiKeySchemeName;

                return JwtBearerDefaults.AuthenticationScheme;
            };
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => 
        { 
            ConfigureJwtBearerOptions(options, jwtConfig, validationKey); 
        })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(apiKeySchemeName, _ => { });
    }

    private static void SetupApiKeyOnlyAuthentication(
        WebApplicationBuilder builder, 
        string apiKeySchemeName)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = apiKeySchemeName;
            options.DefaultChallengeScheme = apiKeySchemeName;
        })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(apiKeySchemeName, _ => { });
    }

    private static void ConfigureJwtBearerOptions(JwtBearerOptions options, IJwtConfiguration jwtConfig, SecurityKey validationKey)
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context => 
                WriteAuthError(context.HttpContext, ErrorResponseHelper.Unauthorized(errorCode: "INVALID_TOKEN")),
        
            OnForbidden = context => 
                WriteAuthError(context.HttpContext, ErrorResponseHelper.Forbidden()),
        
            OnChallenge = context =>
            {
                context.HandleResponse(); // Suppress default logic
                var error = context.AuthenticateFailure != null
                    ? ErrorResponseHelper.Unauthorized(errorCode: "AUTH_REQUIRED")
                    : ErrorResponseHelper.Unauthorized("Authentication required.", "NO_TOKEN");
            
                return WriteAuthError(context.HttpContext, error);
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = validationKey
        };
    }

    private static Task WriteAuthError(HttpContext context, ErrorResponse error)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;
        context.Response.StatusCode = error.Status;
        // Use common JSON options from JsonSetup
        return context.Response.WriteAsJsonAsync(error, JsonSetup.CommonOptions);
    }
}