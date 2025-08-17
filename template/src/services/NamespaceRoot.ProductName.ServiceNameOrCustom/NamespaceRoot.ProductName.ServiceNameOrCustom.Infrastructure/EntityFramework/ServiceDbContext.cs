using Microsoft.EntityFrameworkCore;
using NamespaceRoot.ProductName.Common.Application.Repositories;
using NamespaceRoot.ProductName.Common.Infrastructure.Repositories.EntityFramework;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;

public class ServiceDbContext(DbContextOptions<ServiceDbContext> options, IServiceProvider serviceProvider)
    : DbContextBase(options, serviceProvider), IUnitOfWork
{
    public const string DefaultSchemaName = "tmp";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DefaultSchemaName);

        // Автоматическая регистрация конфигураций из сборки
        //modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}