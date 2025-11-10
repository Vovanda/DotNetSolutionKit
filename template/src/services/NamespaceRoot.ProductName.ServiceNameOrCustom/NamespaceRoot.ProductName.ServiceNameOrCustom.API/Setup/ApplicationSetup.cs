using NamespaceRoot.ProductName.Common.Application.Configuration;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Application;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class ApplicationSetup
{
    public static WebApplicationBuilder SetupAppServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;
        
        // services.AddJwtConfiguration(configuration);
        services.AddInternalApiConfiguration(configuration);
        services.AddAuthValidationConfiguration(configuration);

        // Register Domain Layer services
        services.AddDomainServices();
        
        // Register Application Layer services
        services.AddApplicationServices();
        
        // Register Infrastructure Layer services
        services.AddInfrastructureServices(builder.Configuration);

        return builder;
    }
}