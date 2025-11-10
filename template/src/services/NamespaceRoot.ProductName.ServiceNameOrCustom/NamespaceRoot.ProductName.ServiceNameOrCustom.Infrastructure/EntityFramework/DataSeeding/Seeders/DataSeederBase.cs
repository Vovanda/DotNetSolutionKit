using Microsoft.Extensions.Logging;
using NamespaceRoot.ProductName.Common.Contracts.Domain.Context;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework.DataSeeding.Seeders;

public abstract class DataSeederBase
{
    protected readonly ILogger Logger;
    protected readonly ServiceNameOrCustomDbContext DbContext;
    protected readonly IDomainExecutionContext SeedContext;

    protected DataSeederBase(
        ServiceNameOrCustomDbContext dbContext,
        ILogger logger)
    {
        Logger = logger;
        DbContext = dbContext;
        SeedContext = new SystemSeedContext();
    }

    public abstract Task SeedAsync(CancellationToken cancellationToken = default);
}