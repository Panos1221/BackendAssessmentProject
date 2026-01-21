using BackendProject.Domain.Interfaces;

namespace BackendProject.Application.Common;

/// <summary>
/// Extension methods for executing operations within database transactions.
/// </summary>
public static class TransactionExtensions
{
    /// <summary>
    /// Executes an operation within a transaction, automatically handling commit and rollback.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="unitOfWork">The unit of work to use for the transaction.</param>
    /// <param name="operation">The async operation to execute within the transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        this IUnitOfWork unitOfWork,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation();
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Executes an operation within a transaction, automatically handling commit and rollback.
    /// </summary>
    /// <param name="unitOfWork">The unit of work to use for the transaction.</param>
    /// <param name="operation">The async operation to execute within the transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task ExecuteInTransactionAsync(
        this IUnitOfWork unitOfWork,
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation();
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
