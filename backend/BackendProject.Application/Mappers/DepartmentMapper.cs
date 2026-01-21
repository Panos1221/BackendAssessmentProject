using System.Linq.Expressions;
using BackendProject.Application.DTOs;
using BackendProject.Domain.Entities;

namespace BackendProject.Application.Mappers;

/// <summary>
/// Static mapper for Department entity to response DTOs.
/// </summary>
public static class DepartmentMapper
{
    /// <summary>
    /// Expression for projecting Department to DepartmentResponse (for use in LINQ queries).
    /// </summary>
    public static Expression<Func<Department, DepartmentResponse>> ToResponse => d => new DepartmentResponse
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        EmployeeCount = d.Employees.Count(e => !e.IsDeleted)
    };

    /// <summary>
    /// Maps a Department entity to DepartmentResponse DTO.
    /// </summary>
    public static DepartmentResponse MapToResponse(Department department) => new()
    {
        Id = department.Id,
        Name = department.Name,
        Description = department.Description,
        EmployeeCount = department.Employees?.Count(e => !e.IsDeleted) ?? 0
    };
}
