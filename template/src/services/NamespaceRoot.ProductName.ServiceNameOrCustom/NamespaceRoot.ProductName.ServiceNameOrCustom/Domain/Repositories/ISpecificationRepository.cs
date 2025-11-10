using LinqSpecs;
using NamespaceRoot.ProductName.Common.Contracts.Requests;
using NamespaceRoot.ProductName.Common.Domain;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Domain.Repositories;

/// <summary>
/// Provides a standard contract for querying and persisting domain entities.
/// </summary>
public interface ISpecificationRepository<T> : IRepositoryBase where T : Entity
{
    /// <summary>
    /// Searches for entities based on business specifications with pagination and sorting.
    /// </summary>
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetBySpecificationAsync(
        Specification<T> spec,
        IPaginationRequest paging,
        ISortableRequest sorting,
        CancellationToken cancellationToken = default);
}