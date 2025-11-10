using System.Linq.Dynamic.Core; 
using NamespaceRoot.ProductName.Common.Contracts.Requests;

namespace NamespaceRoot.ProductName.Common.Application.Extensions;

/// <summary>
/// Common extensions for IQueryable to handle sorting and pagination using standard interfaces.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies dynamic sorting based on property name and direction.
    /// Requires System.Linq.Dynamic.Core.
    /// </summary>
    public static IOrderedQueryable<T> ApplySorting<T>(
        this IQueryable<T> query, 
        ISortableRequest request, 
        IReadOnlyDictionary<string, string> mappings,
        string defaultSort = "Id")
    {
        if (string.IsNullOrWhiteSpace(request.SortBy) || !mappings.TryGetValue(request.SortBy.ToLower(), out var dbField))
        {
            return query.OrderBy($"{defaultSort} ascending");
        }

        var direction = request.SortDir == SortDirection.Desc ? "descending" : "ascending";
    
        // Dynamic LINQ
        return query.OrderBy($"{dbField} {direction}");
    }

    /// <summary>
    /// Applies Skip and Take based on page number and page size.
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, IPaginationRequest request)
    {
        // Simple safety check for 2026 standards
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
}