namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class WebApi
{
    public static WebApplication UseWebApi(this WebApplication app)
    {
        app.MapControllers();
        return app;
    }
}