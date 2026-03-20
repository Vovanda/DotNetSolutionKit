using System.Reflection;
using NamespaceRoot.ProductName.Common.Web.Swagger;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Swagger;

internal static class SwaggerSetup
{
    public static WebApplicationBuilder SetupSwaggerPage(this WebApplicationBuilder builder) =>
        builder.SetupSwaggerPage(Assembly.GetExecutingAssembly());

    public static WebApplication UseSwaggerPage(this WebApplication app) =>
        app.UseSwaggerPage(Assembly.GetExecutingAssembly());
}
