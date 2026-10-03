using System.Reflection;
using NamespaceRoot.ProductName.Common.Web.Setup;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Validation
{
    /// <summary>
    /// Registers the request validators this service declares in its API project.
    /// </summary>
    public static WebApplicationBuilder SetupValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidation(Assembly.GetExecutingAssembly());
        return builder;
    }
}
