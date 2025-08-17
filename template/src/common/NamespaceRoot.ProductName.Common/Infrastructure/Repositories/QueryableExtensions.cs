namespace NamespaceRoot.ProductName.Common.Infrastructure.Repositories;

internal static class QueryableExtensions
{
    /// <summary>
    /// Применяет include-функцию, если она задана.
    /// Позволяет использовать Include / ThenInclude-цепочки.
    /// </summary>
    public static IQueryable<TEntity> ApplyIncludes<TEntity>(
        this IQueryable<TEntity> query,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include)
        where TEntity : class
    {
        return include is null ? query : include(query);
    }
}