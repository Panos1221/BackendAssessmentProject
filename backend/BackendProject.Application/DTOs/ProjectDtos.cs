namespace BackendProject.Application.DTOs;

// NOTE: Σε μεγαλύτερα project αυτά τα DTOs θα ήταν καλύτερο να ήταν σε ξεχωριστά αρχεία.


/// <summary>
/// Request DTO for creating a new project.
/// </summary>
public class CreateProjectRequest
{
    /// <example>Backend Developer Technical Assessment</example>
    public string Name { get; set; } = string.Empty;
    
    /// <example>Technical assessment project for evaluating backend development skills</example>
    public string? Description { get; set; }
    
    /// <example>2024-01-01T00:00:00Z</example>
    public DateTime StartDate { get; set; }
    
    /// <example>2024-12-31T00:00:00Z</example>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Request DTO for updating an existing project.
/// </summary>
public class UpdateProjectRequest
{
    /// <summary>
    /// The ID of the project being updated. Set internally from route parameter.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid Id { get; set; }
    
    /// <example>Backend Developer Technical Assessment</example>
    public string Name { get; set; } = string.Empty;
    
    /// <example>Technical assessment project for evaluating backend development skills - Updated</example>
    public string? Description { get; set; }
    
    /// <example>2024-01-01T00:00:00Z</example>
    public DateTime StartDate { get; set; }
    
    /// <example>2024-12-31T00:00:00Z</example>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Response DTO for project data.
/// </summary>
public class ProjectResponse
{
    /// <example>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</example>
    public Guid Id { get; set; }
    /// <example>Backend Developer Technical Assessment</example>
    public string Name { get; set; } = string.Empty;
    /// <example>Technical assessment project for evaluating backend development skills</example>
    public string? Description { get; set; }
    /// <example>2024-01-01T00:00:00Z</example>
    public DateTime StartDate { get; set; }
    /// <example>2024-12-31T00:00:00Z</example>
    public DateTime? EndDate { get; set; }
    /// <example>3</example>
    public int EmployeeCount { get; set; }
}
