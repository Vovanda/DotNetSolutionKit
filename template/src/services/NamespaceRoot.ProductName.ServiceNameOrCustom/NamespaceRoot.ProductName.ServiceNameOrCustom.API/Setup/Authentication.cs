using NamespaceRoot.ProductName.Common.Web.Authentication;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.Security.Handlers;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

/// <summary>
/// Authentication and authorization for this service: the API key forwarded by the gateway, with JWT as
/// an optional fallback when a "Jwt" section is configured. The handler lives in this service's
/// infrastructure; the shared setup is <see cref="ServiceAuthenticationSetup"/>. The middleware is added
/// by the platform pipeline.
/// </summary>
internal static class Authentication
{
    public static WebApplicationBuilder SetupAppAuthentication(this WebApplicationBuilder builder)
    {
        builder.SetupServiceAuthentication<ApiKeyAuthenticationHandler>();
        builder.Services.AddAuthorization();
        return builder;
    }
}
