using System.Linq.Expressions;
using BackendProject.Domain.Common;

namespace BackendProject.Domain.Interfaces;

/// <summary>
/// Generic repository interface for CRUD operations with soft delete support.
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Add a new entity.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing entity.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Soft delete an entity by setting IsDeleted flag.
    /// </summary>
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an entity exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any entity matches the predicate.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queryable for complex queries.
    /// </summary>
    IQueryable<T> Query();
}
