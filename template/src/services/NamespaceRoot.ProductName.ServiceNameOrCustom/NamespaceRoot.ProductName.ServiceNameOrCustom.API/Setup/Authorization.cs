namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Authorization
{
    public static WebApplication UseAppAuthorization(this WebApplication app)
    {
        app.UseAuthorization();
        return app;
    }
    
    public static WebApplicationBuilder SetupApplicationAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(/* Можно добавить политики */);

        return builder;
    }
}