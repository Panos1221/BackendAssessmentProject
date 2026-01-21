using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Application.Common;

/// <summary>
/// Extension methods for IQueryable to simplify common query patterns.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination and projection to a query, returning a paginated result.
    /// </summary>
    /// <typeparam name="TSource">The source entity type.</typeparam>
    /// <typeparam name="TResult">The projected result type.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="pagination">The pagination parameters.</param>
    /// <param name="selector">The projection expression.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated result containing the projected items.</returns>
    public static async Task<PaginatedResult<TResult>> ToPaginatedResultAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        PaginationParams pagination,
        Expression<Func<TSource, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TResult>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }
}
