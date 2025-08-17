using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Authentication
{
    public static WebApplicationBuilder SetupApplicationAuthentication(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Jwt:Authority"];
                options.Audience = configuration["Jwt:Audience"];
                options.RequireHttpsMetadata = false;
            });

        return builder;
    }
}