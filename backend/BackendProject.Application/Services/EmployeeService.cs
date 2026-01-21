using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Interfaces;
using BackendProject.Application.Mappers;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<EmployeeResponse>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Employees.Query()
            .Include(e => e.Department);

        return await query.ToPaginatedResultAsync(pagination, EmployeeMapper.ToResponse, cancellationToken);
    }

    public async Task<EmployeeDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.Department)
            .Include(e => e.EmployeeProjects)
                .ThenInclude(ep => ep.Project)
                    .ThenInclude(p => p.EmployeeProjects)
                        .ThenInclude(ep => ep.Employee)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found");

        return EmployeeMapper.MapToDetailResponse(employee);
    }

    public async Task<PaginatedResult<EmployeeResponse>> SearchAsync(string searchTerm, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Employees.Query()
            .Include(e => e.Department)
            .Where(e => EF.Functions.Like(e.FirstName, $"%{searchTerm}%") ||
                        EF.Functions.Like(e.LastName, $"%{searchTerm}%") ||
                        EF.Functions.Like(e.Email, $"%{searchTerm}%"));

        return await query.ToPaginatedResultAsync(pagination, EmployeeMapper.ToResponse, cancellationToken);
    }

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Status = request.Status,
                HireDate = request.HireDate,
                Notes = request.Notes,
                DepartmentId = request.DepartmentId
            };

            await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var createdEmployee = await _unitOfWork.Employees.Query()
                .Include(e => e.Department)
                .FirstAsync(e => e.Id == employee.Id, cancellationToken);

            return EmployeeMapper.MapToResponse(createdEmployee);
        }, cancellationToken);
    }

    public async Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employee = await _unitOfWork.Employees.Query()
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Employee with ID {id} not found");

            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.Email = request.Email;
            employee.Status = request.Status;
            employee.HireDate = request.HireDate;
            employee.Notes = request.Notes;
            employee.DepartmentId = request.DepartmentId;

            _unitOfWork.Employees.Update(employee);

            // Reload to get updated department
            var updatedEmployee = await _unitOfWork.Employees.Query()
                .Include(e => e.Department)
                .FirstAsync(e => e.Id == id, cancellationToken);

            return EmployeeMapper.MapToResponse(updatedEmployee);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var exists = await _unitOfWork.Employees.ExistsAsync(id, cancellationToken);
            if (!exists)
                throw new KeyNotFoundException($"Employee with ID {id} not found");

            await _unitOfWork.Employees.SoftDeleteAsync(id, cancellationToken);
        }, cancellationToken);
    }

    public async Task AssignToProjectAsync(Guid employeeId, Guid projectId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employeeExists = await _unitOfWork.Employees.ExistsAsync(employeeId, cancellationToken);
            if (!employeeExists)
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

            var projectExists = await _unitOfWork.Projects.ExistsAsync(projectId, cancellationToken);
            if (!projectExists)
                throw new KeyNotFoundException($"Project with ID {projectId} not found");

            var employee = await _unitOfWork.Employees.Query()
                .Include(e => e.EmployeeProjects)
                .FirstAsync(e => e.Id == employeeId, cancellationToken);

            // Check if already assigned - idempotent operation
            if (employee.EmployeeProjects.Any(ep => ep.ProjectId == projectId))
                return;

            employee.EmployeeProjects.Add(new EmployeeProject
            {
                EmployeeId = employeeId,
                ProjectId = projectId
            });
        }, cancellationToken);
    }

    public async Task RemoveFromProjectAsync(Guid employeeId, Guid projectId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employee = await _unitOfWork.Employees.Query()
                .Include(e => e.EmployeeProjects)
                .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken)
                ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

            var assignment = employee.EmployeeProjects.FirstOrDefault(ep => ep.ProjectId == projectId)
                ?? throw new KeyNotFoundException($"Assignment between employee {employeeId} and project {projectId} not found");

            employee.EmployeeProjects.Remove(assignment);
        }, cancellationToken);
    }
}
