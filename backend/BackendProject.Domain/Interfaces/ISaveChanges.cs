namespace BackendProject.Domain.Interfaces;

/// <summary>
/// Interface for persisting tracked changes to the data store.
/// </summary>
public interface ISaveChanges
{
    /// <summary>
    /// Saves all changes made in the context to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
