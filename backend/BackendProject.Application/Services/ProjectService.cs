using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Interfaces;
using BackendProject.Application.Mappers;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<ProjectResponse>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Projects.Query()
            .Include(p => p.EmployeeProjects)
                .ThenInclude(ep => ep.Employee);

        return await query.ToPaginatedResultAsync(pagination, ProjectMapper.ToResponse, cancellationToken);
    }

    public async Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.Query()
            .Include(p => p.EmployeeProjects)
                .ThenInclude(ep => ep.Employee)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project with ID {id} not found");

        return ProjectMapper.MapToResponse(project);
    }

    public async Task<PaginatedResult<ProjectResponse>> SearchAsync(string searchTerm, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Projects.Query()
            .Include(p => p.EmployeeProjects)
                .ThenInclude(ep => ep.Employee)
            .Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                        (p.Description != null && EF.Functions.Like(p.Description, $"%{searchTerm}%")));

        return await query.ToPaginatedResultAsync(pagination, ProjectMapper.ToResponse, cancellationToken);
    }

    public async Task<PaginatedResult<EmployeeResponse>> GetEmployeesAsync(Guid projectId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var projectExists = await _unitOfWork.Projects.ExistsAsync(projectId, cancellationToken);
        if (!projectExists)
            throw new KeyNotFoundException($"Project with ID {projectId} not found");

        var query = _unitOfWork.Employees.Query()
            .Include(e => e.Department)
            .Include(e => e.EmployeeProjects)
            .Where(e => e.EmployeeProjects.Any(ep => ep.ProjectId == projectId));

        return await query.ToPaginatedResultAsync(pagination, EmployeeMapper.ToResponse, cancellationToken);
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            await _unitOfWork.Projects.AddAsync(project, cancellationToken);

            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                EmployeeCount = 0
            };
        }, cancellationToken);
    }

    public async Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var project = await _unitOfWork.Projects.Query()
                .Include(p => p.EmployeeProjects)
                    .ThenInclude(ep => ep.Employee)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Project with ID {id} not found");

            project.Name = request.Name;
            project.Description = request.Description;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;

            _unitOfWork.Projects.Update(project);

            return ProjectMapper.MapToResponse(project);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var exists = await _unitOfWork.Projects.ExistsAsync(id, cancellationToken);
            if (!exists)
                throw new KeyNotFoundException($"Project with ID {id} not found");

            await _unitOfWork.Projects.SoftDeleteAsync(id, cancellationToken);
        }, cancellationToken);
    }
}
