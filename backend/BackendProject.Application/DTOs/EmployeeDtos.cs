using BackendProject.Domain.Enums;

namespace BackendProject.Application.DTOs;

// NOTE: �� ���������� project ���� �� DTOs �� ���� �������� �� ���� �� ��������� ������.

/// <summary>
/// Request DTO for creating a new employee.
/// </summary>
public class CreateEmployeeRequest
{
    /// <example>Panagiotis</example>
    public string FirstName { get; set; } = string.Empty;
    
    /// <example>Stavrakellis</example>
    public string LastName { get; set; } = string.Empty;
    
    /// <example>panagiotis.stavrakellis@company.com</example>
    public string Email { get; set; } = string.Empty;
    
    /// <example>0</example>
    public EmployeeStatus Status { get; set; }
    
    /// <example>2024-01-15T00:00:00Z</example>
    public DateTime HireDate { get; set; }
    
    /// <example>Backend Developer</example>
    public string? Notes { get; set; }
    
    /// <example>11111111-1111-1111-1111-111111111111</example>
    public Guid DepartmentId { get; set; }
}

/// <summary>
/// Request DTO for updating an existing employee.
/// </summary>
public class UpdateEmployeeRequest
{
    /// <summary>
    /// The ID of the employee being updated. Set internally from route parameter.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid Id { get; set; }
    
    /// <example>Panagiotis</example>
    public string FirstName { get; set; } = string.Empty;
    
    /// <example>Stavrakellis</example>
    public string LastName { get; set; } = string.Empty;
    
    /// <example>panagiotis.stavrakellis@company.com</example>
    public string Email { get; set; } = string.Empty;
    
    /// <example>0</example>
    public EmployeeStatus Status { get; set; }
    
    /// <example>2024-01-15T00:00:00Z</example>
    public DateTime HireDate { get; set; }
    
    /// <example>Senior Backend Developer</example>
    public string? Notes { get; set; }
    
    /// <example>11111111-1111-1111-1111-111111111111</example>
    public Guid DepartmentId { get; set; }
}

/// <summary>
/// Response DTO for employee list items.
/// </summary>
public class EmployeeResponse
{
    /// <example>dddddddd-dddd-dddd-dddd-dddddddddddd</example>
    public Guid Id { get; set; }
    /// <example>Panagiotis</example>
    public string FirstName { get; set; } = string.Empty;
    /// <example>Stavrakellis</example>
    public string LastName { get; set; } = string.Empty;
    /// <example>panagiotis.stavrakellis@company.com</example>
    public string Email { get; set; } = string.Empty;
    /// <example>0</example>
    public EmployeeStatus Status { get; set; }
    /// <example>2024-01-15T00:00:00Z</example>
    public DateTime HireDate { get; set; }
    /// <example>Backend Developer</example>
    public string? Notes { get; set; }
    /// <example>11111111-1111-1111-1111-111111111111</example>
    public Guid DepartmentId { get; set; }
    /// <example>Backend Developing</example>
    public string DepartmentName { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for detailed employee view including projects.
/// </summary>
public class EmployeeDetailResponse : EmployeeResponse
{
    /// <summary>
    /// List of projects assigned to this employee.
    /// </summary>
    public ICollection<ProjectResponse> Projects { get; set; } = new List<ProjectResponse>();
}
