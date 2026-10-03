using Microsoft.EntityFrameworkCore;
using NamespaceRoot.ProductName.Common.Domain;
using NamespaceRoot.ProductName.Common.Domain.Context;
using NamespaceRoot.ProductName.Common.Domain.Events;
using NamespaceRoot.ProductName.Common.Domain.Persistence;
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework;


namespace NamespaceRoot.ProductName.Common.Tests.Fixtures;

/// <summary>
/// A two-table model standing in for a real service: an aggregate root with a relation to load, and one
/// repository built on the shared base. Deliberately dull - the tests are about the base class, not about
/// this domain.
/// </summary>
public class Region : Entity<Guid>
{
    public Region(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Name { get; private set; }

    public List<Plan> Plans { get; private set; } = [];
}

public class Plan : EventfulEntity<Guid>, IAggregateRoot
{
    public Plan(Guid id, string name, int priceMinor, bool isActive, Region region)
    {
        Id = id;
        Name = name;
        PriceMinor = priceMinor;
        IsActive = isActive;
        Region = region;
        RegionId = region.Id;
    }

    private Plan()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public int PriceMinor { get; private set; }

    public bool IsActive { get; private set; }

    public Guid RegionId { get; private set; }

    public Region Region { get; private set; } = null!;

    /// <summary>
    /// A change that both edits the row and announces itself, so that discarding the work can be
    /// checked to drop the announcement along with the edit.
    /// </summary>
    public void Reprice(int priceMinor, IDomainExecutionContext context)
    {
        PriceMinor = priceMinor;
        AddDomainEvent(new PlanRepriced(context, priceMinor));
    }
}

/// <summary>
/// The one event this stand-in domain raises.
/// </summary>
public sealed record PlanRepriced(IDomainExecutionContext Context, int PriceMinor) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = Context.TimeProvider.GetUtcNow();
}

public class CatalogueDbContext : DbContextBase
{
    public CatalogueDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Plan> Plans => Set<Plan>();

    public DbSet<Region> Regions => Set<Region>();
}

