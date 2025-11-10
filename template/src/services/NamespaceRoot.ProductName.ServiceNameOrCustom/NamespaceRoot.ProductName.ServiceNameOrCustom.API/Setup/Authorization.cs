namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Authorization
{
    public static WebApplication UseAppAuthorization(this WebApplication app)
    {
        app.UseAuthorization();
        return app;
    }
    
    public static WebApplicationBuilder SetupAppAuthorization(this WebApplicationBuilder builder)
    {
        // UseAuthorization must be called after UseAuthentication
        builder.Services.AddAuthorization();
        return builder;
    }
}