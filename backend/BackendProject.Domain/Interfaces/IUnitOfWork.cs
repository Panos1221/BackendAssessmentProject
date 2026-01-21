using BackendProject.Domain.Entities;

namespace BackendProject.Domain.Interfaces;

/// <summary>
/// Unit of Work interface for coordinating multiple repository operations
/// and managing transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Employee> Employees { get; }
    IRepository<Department> Departments { get; }
    IRepository<Project> Projects { get; }

    /// <summary>
    /// Begin a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all changes made in this unit of work to the database.
    /// Requires an active transaction (call BeginTransactionAsync() first).
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
