namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class HealthChecks
{
    public static WebApplicationBuilder SetupHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        return builder;
    }
}