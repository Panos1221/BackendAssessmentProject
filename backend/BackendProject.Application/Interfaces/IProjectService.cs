using BackendProject.Application.Common;
using BackendProject.Application.DTOs;

namespace BackendProject.Application.Interfaces;

public interface IProjectService
{
    Task<PaginatedResult<ProjectResponse>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<ProjectResponse>> SearchAsync(string searchTerm, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<PaginatedResult<EmployeeResponse>> GetEmployeesAsync(Guid projectId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
