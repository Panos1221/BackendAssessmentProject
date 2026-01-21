namespace BackendProject.Domain.Common;

/// <summary>
/// Base entity with common properties for all entities.
/// Includes soft delete support.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Soft delete flag. When true, the entity is considered deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
