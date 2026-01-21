namespace BackendProject.Application.DTOs;

// NOTE: Σε μεγαλύτερα project αυτά τα DTOs θα ήταν καλύτερο να ήταν σε ξεχωριστά αρχεία.

/// <summary>
/// Request DTO for creating a new department.
/// </summary>
public class CreateDepartmentRequest
{
    /// <example>Backend Developing</example>
    public string Name { get; set; } = string.Empty;
    
    /// <example>Backend Developing department</example>
    public string? Description { get; set; }
}

/// <summary>
/// Request DTO for updating an existing department.
/// </summary>
public class UpdateDepartmentRequest
{
    /// <summary>
    /// The ID of the department being updated. Set internally from route parameter.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid Id { get; set; }
    
    /// <example>Backend Developing</example>
    public string Name { get; set; } = string.Empty; 
    /// <example>Backend Developing department - Updated</example>
    public string? Description { get; set; }
}

/// <summary>
/// Response DTO for department data.
/// </summary>
public class DepartmentResponse
{
    /// <example>11111111-1111-1111-1111-111111111111</example>
    public Guid Id { get; set; }
    /// <example>Backend Developing</example>
    public string Name { get; set; } = string.Empty;
    /// <example>Backend Developing department</example>
    public string? Description { get; set; }
    /// <example>3</example>
    public int EmployeeCount { get; set; }
}
