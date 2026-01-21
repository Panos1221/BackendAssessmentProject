using BackendProject.Application.Common;
using BackendProject.Application.DTOs;

namespace BackendProject.Application.Interfaces;

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeResponse>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<EmployeeDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<EmployeeResponse>> SearchAsync(string searchTerm, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AssignToProjectAsync(Guid employeeId, Guid projectId, CancellationToken cancellationToken = default);
    Task RemoveFromProjectAsync(Guid employeeId, Guid projectId, CancellationToken cancellationToken = default);
}
