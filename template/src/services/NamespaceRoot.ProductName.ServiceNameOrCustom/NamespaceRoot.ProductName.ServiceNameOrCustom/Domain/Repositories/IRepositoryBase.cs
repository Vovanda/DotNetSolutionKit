using NamespaceRoot.ProductName.Common.Domain.Persistence;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Domain.Repositories;

public interface IRepositoryBase
{
    /// <summary>
    /// UnitOfWork associated with this repository instance
    /// Use for saving changes in simple operations
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}