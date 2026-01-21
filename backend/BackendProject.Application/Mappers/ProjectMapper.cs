using System.Linq.Expressions;
using BackendProject.Application.DTOs;
using BackendProject.Domain.Entities;

namespace BackendProject.Application.Mappers;

/// <summary>
/// Static mapper for Project entity to response DTOs.
/// </summary>
public static class ProjectMapper
{
    /// <summary>
    /// Expression for projecting Project to ProjectResponse (for use in LINQ queries).
    /// </summary>
    public static Expression<Func<Project, ProjectResponse>> ToResponse => p => new ProjectResponse
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        EmployeeCount = p.EmployeeProjects.Count(ep => !ep.Employee.IsDeleted)
    };

    /// <summary>
    /// Maps a Project entity to ProjectResponse DTO.
    /// </summary>
    public static ProjectResponse MapToResponse(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        StartDate = project.StartDate,
        EndDate = project.EndDate,
        EmployeeCount = project.EmployeeProjects?.Count(ep => !ep.Employee?.IsDeleted ?? true) ?? 0
    };
}
