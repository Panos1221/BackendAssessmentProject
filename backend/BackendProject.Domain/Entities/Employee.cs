using BackendProject.Domain.Common;
using BackendProject.Domain.Enums;

namespace BackendProject.Domain.Entities;

/// <summary>
/// Represents an employee in the system.
/// </summary>
public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EmployeeStatus Status { get; set; }
    public DateTime HireDate { get; set; }
    public string? Notes { get; set; }

    // Foreign key for Department (One-to-Many)
    public Guid DepartmentId { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
}
