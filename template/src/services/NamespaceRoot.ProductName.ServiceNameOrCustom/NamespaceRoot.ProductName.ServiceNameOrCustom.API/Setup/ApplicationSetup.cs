using NamespaceRoot.ProductName.ServiceNameOrCustom.Application;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class ApplicationSetup
{
    public static WebApplicationBuilder SetupWebApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        return builder;
    }

    public static WebApplicationBuilder SetupAppServices(this WebApplicationBuilder builder)
    {
        // Регистрация MediatR обработчиков только из Application сборки
        var applicationAssembly = typeof(ApplicationMarker).Assembly; 
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(applicationAssembly));

        // Регистрация инфраструктурных сервисов
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
        builder.Services.AddInfrastructureServices(connectionString);

        return builder;
    }
}