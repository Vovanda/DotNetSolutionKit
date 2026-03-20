using NamespaceRoot.ProductName.Common.Web.Authentication;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.Security.Handlers;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

/// <summary>
/// Authentication setup for this service.
/// Primary: internal API Key forwarded by the Gateway.
/// JWT: optional fallback when "Jwt" section is configured (dev/local only).
/// See <see cref="ServiceAuthenticationSetup"/> for the shared implementation.
/// </summary>
internal static class Authentication
{
    public static WebApplicationBuilder SetupAppAuthentication(this WebApplicationBuilder builder) =>
        builder.SetupServiceAuthentication<ApiKeyAuthenticationHandler>();

    public static WebApplication UseAppAuthentication(this WebApplication app) =>
        app.UseServiceAuthentication();
}
