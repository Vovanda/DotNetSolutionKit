using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NamespaceRoot.ProductName.Common.Application.Repositories;
using NamespaceRoot.ProductName.Common.DependencyInjection;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure;

/// <summary>
/// Расширения для регистрации инфраструктурных сервисов в DI контейнере.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрация сервисов инфраструктуры.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="connectionString">Строка подключения к базе данных.</param>
    /// <returns>Обновленная коллекция сервисов.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        // Регистрация сервисов поиска с учётом регистра

        // Регистрация DbContext
        services.AddDbContextFactory<ServiceDbContext>(options =>
            options.UseNpgsql(connectionString, x =>
            {
                x.MigrationsHistoryTable("__EFMigrationsHistory", ServiceDbContext.DefaultSchemaName);
            })
        );
        services.AddDbContext<ServiceDbContext>(ServiceLifetime.Scoped);
        services.AddScoped<ServiceDbContext>().Aka<IUnitOfWork>();
        

        // Регистрация репозиториев

        return services;
    }
}
