using BackendProject.Domain.Common;

namespace BackendProject.Domain.Entities;

/// <summary>
/// Represents a project that can have multiple employees assigned.
/// </summary>
public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Navigation property (Many-to-Many via join entity)
    public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
}
