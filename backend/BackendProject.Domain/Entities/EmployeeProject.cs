namespace BackendProject.Domain.Entities;

/// <summary>
/// Join entity for the many-to-many relationship between Employee and Project.
/// </summary>
public class EmployeeProject
{
    public Guid EmployeeId { get; set; }
    public Guid ProjectId { get; set; }

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
