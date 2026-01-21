using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Interfaces;
using BackendProject.Application.Mappers;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<DepartmentResponse>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Departments.Query()
            .Include(d => d.Employees);

        return await query.ToPaginatedResultAsync(pagination, DepartmentMapper.ToResponse, cancellationToken);
    }

    public async Task<DepartmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.Query()
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Department with ID {id} not found");

        return DepartmentMapper.MapToResponse(department);
    }

    public async Task<PaginatedResult<DepartmentResponse>> SearchAsync(string searchTerm, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Departments.Query()
            .Include(d => d.Employees)
            .Where(d => EF.Functions.Like(d.Name, $"%{searchTerm}%") ||
                        (d.Description != null && EF.Functions.Like(d.Description, $"%{searchTerm}%")));

        return await query.ToPaginatedResultAsync(pagination, DepartmentMapper.ToResponse, cancellationToken);
    }

    public async Task<PaginatedResult<EmployeeResponse>> GetEmployeesAsync(Guid departmentId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var departmentExists = await _unitOfWork.Departments.ExistsAsync(departmentId, cancellationToken);
        if (!departmentExists)
            throw new KeyNotFoundException($"Department with ID {departmentId} not found");

        var query = _unitOfWork.Employees.Query()
            .Include(e => e.Department)
            .Where(e => e.DepartmentId == departmentId);

        return await query.ToPaginatedResultAsync(pagination, EmployeeMapper.ToResponse, cancellationToken);
    }

    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description
            };

            await _unitOfWork.Departments.AddAsync(department, cancellationToken);

            return new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                EmployeeCount = 0
            };
        }, cancellationToken);
    }

    public async Task<DepartmentResponse> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var department = await _unitOfWork.Departments.Query()
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Department with ID {id} not found");

            department.Name = request.Name;
            department.Description = request.Description;

            _unitOfWork.Departments.Update(department);

            return DepartmentMapper.MapToResponse(department);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var exists = await _unitOfWork.Departments.ExistsAsync(id, cancellationToken);
            if (!exists)
                throw new KeyNotFoundException($"Department with ID {id} not found");

            await _unitOfWork.Departments.SoftDeleteAsync(id, cancellationToken);
        }, cancellationToken);
    }
}
