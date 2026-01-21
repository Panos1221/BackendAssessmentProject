using BackendProject.Application.Common;
using BackendProject.Application.DTOs;

namespace BackendProject.Application.Interfaces;

public interface IDepartmentService
{
    Task<PaginatedResult<DepartmentResponse>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<DepartmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<DepartmentResponse>> SearchAsync(string searchTerm, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<PaginatedResult<EmployeeResponse>> GetEmployeesAsync(Guid departmentId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<DepartmentResponse> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
