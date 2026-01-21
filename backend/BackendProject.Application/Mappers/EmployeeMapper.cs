using System.Linq.Expressions;
using BackendProject.Application.DTOs;
using BackendProject.Domain.Entities;

namespace BackendProject.Application.Mappers;

/// <summary>
/// Static mapper for Employee entity to response DTOs.
/// </summary>
public static class EmployeeMapper
{
    /// <summary>
    /// Expression for projecting Employee to EmployeeResponse (for use in LINQ queries).
    /// </summary>
    public static Expression<Func<Employee, EmployeeResponse>> ToResponse => e => new EmployeeResponse
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Status = e.Status,
        HireDate = e.HireDate,
        Notes = e.Notes,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department.Name
    };

    /// <summary>
    /// Maps an Employee entity to EmployeeResponse DTO.
    /// </summary>
    public static EmployeeResponse MapToResponse(Employee employee) => new()
    {
        Id = employee.Id,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Email = employee.Email,
        Status = employee.Status,
        HireDate = employee.HireDate,
        Notes = employee.Notes,
        DepartmentId = employee.DepartmentId,
        DepartmentName = employee.Department?.Name ?? string.Empty
    };

    /// <summary>
    /// Maps an Employee entity to EmployeeDetailResponse DTO including projects.
    /// </summary>
    public static EmployeeDetailResponse MapToDetailResponse(Employee employee) => new()
    {
        Id = employee.Id,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Email = employee.Email,
        Status = employee.Status,
        HireDate = employee.HireDate,
        Notes = employee.Notes,
        DepartmentId = employee.DepartmentId,
        DepartmentName = employee.Department?.Name ?? string.Empty,
        Projects = employee.EmployeeProjects?
            .Where(ep => !ep.Project.IsDeleted)
            .Select(ep => ProjectMapper.MapToResponse(ep.Project))
            .ToList() ?? []
    };
}
