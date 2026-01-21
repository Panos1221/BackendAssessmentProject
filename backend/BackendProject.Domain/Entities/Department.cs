using BackendProject.Domain.Common;

namespace BackendProject.Domain.Entities;

/// <summary>
/// Represents a department that can have multiple employees.
/// </summary>
public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property (One-to-Many)
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
