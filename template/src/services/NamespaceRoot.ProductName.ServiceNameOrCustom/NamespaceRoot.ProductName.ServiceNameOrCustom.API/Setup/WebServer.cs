namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class WebServer
{
    internal static WebApplication UseWebServer(this WebApplication app)
    {
        app.UseRouting();
        return app;
    }
}