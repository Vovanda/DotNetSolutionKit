using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NamespaceRoot.ProductName.Common.Domain.Persistence;
using NamespaceRoot.ProductName.Common.Infrastructure.Repositories.EntityFramework;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;

[SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
public class ServiceNameOrCustomDbContext(DbContextOptions<ServiceNameOrCustomDbContext> options)
    : DbContextBase(options), IUnitOfWork
{
    public static readonly string DefaultSchemaName = "ServiceNameOrCustom".ToLowerInvariant();
    
    // Add DbSet here

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DefaultSchemaName);

        // Automatic registration of configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}