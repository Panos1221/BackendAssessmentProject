using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Services;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Enums;
using BackendProject.Tests.TestFixture;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Services;

public class ProjectServiceTests : ServiceTestBase
{
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _projectService = new ProjectService(UnitOfWork);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Test Department"
        };
        Context.Departments.Add(department);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Backend Assessment",
            Description = "Technical Assessment",
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow.AddMonths(3)
        };
        Context.Projects.Add(project);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };
        Context.Employees.Add(employee);

        Context.EmployeeProjects.Add(new EmployeeProject
        {
            EmployeeId = employee.Id,
            ProjectId = project.Id
        });

        Context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProjects()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _projectService.GetAllAsync(pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.NotEmpty(result.Items);
        Assert.Equal(pagination.PageNumber, result.PageNumber);
        Assert.Equal(pagination.PageSize, result.PageSize);
        var project = result.Items.First();
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.NotEmpty(project.Name);
        Assert.True(project.EmployeeCount >= 0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProject_ReturnsProject()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();

        // Act
        var result = await _projectService.GetByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Name, result.Name);
        Assert.Equal(project.Description, result.Description);
        Assert.Equal(project.StartDate, result.StartDate);
        Assert.Equal(project.EndDate, result.EndDate);
        Assert.True(result.EmployeeCount > 0);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _projectService.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesProject()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "New Project",
            Description = "New Project Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _projectService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartDate, result.StartDate);
        Assert.Equal(request.EndDate, result.EndDate);
        Assert.Equal(0, result.EmployeeCount);
        
        // Verify it was actually saved to database
        var savedProject = await Context.Projects
            .FirstOrDefaultAsync(p => p.Id == result.Id);
        Assert.NotNull(savedProject);
        Assert.Equal(request.Name, savedProject.Name);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProject_UpdatesProject()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = project.StartDate,
            EndDate = DateTime.UtcNow.AddYears(1)
        };

        // Act
        var result = await _projectService.UpdateAsync(project.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Project", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(project.StartDate, result.StartDate);
        Assert.Equal(request.EndDate, result.EndDate);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateProjectRequest
        {
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _projectService.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task DeleteAsync_ExistingProject_SoftDeletesProject()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();

        // Act
        await _projectService.DeleteAsync(project.Id);

        // Assert
        var deletedProject = await Context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == project.Id);
        Assert.NotNull(deletedProject);
        Assert.True(deletedProject.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _projectService.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetEmployeesAsync_ExistingProject_ReturnsEmployees()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _projectService.GetEmployeesAsync(project.Id, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.NotEmpty(result.Items);
        var employee = result.Items.First();
        Assert.NotNull(employee.DepartmentName);
        Assert.NotEqual(Guid.Empty, employee.DepartmentId);
    }

    [Fact]
    public async Task GetEmployeesAsync_NonExistingProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _projectService.GetEmployeesAsync(Guid.NewGuid(), pagination));
    }

    [Fact]
    public async Task SearchAsync_MatchingTerm_ReturnsResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _projectService.SearchAsync("Backend", pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task SearchAsync_NoMatchingTerm_ReturnsEmpty()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _projectService.SearchAsync("NonExistent", pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedProjects()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();
        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Act
        var result = await _projectService.GetAllAsync(new PaginationParams { PageNumber = 1, PageSize = 10 });

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(result.Items, p => p.Id == project.Id);
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();
        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _projectService.GetByIdAsync(project.Id));
    }

    [Fact]
    public async Task CreateAsync_ProjectWithNullEndDate_CreatesProject()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Ongoing Project",
            Description = "Project with no end date",
            StartDate = DateTime.UtcNow,
            EndDate = null
        };

        // Act
        var result = await _projectService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartDate, result.StartDate);
        Assert.Null(result.EndDate);
        Assert.Equal(0, result.EmployeeCount);
    }

    [Fact]
    public async Task UpdateAsync_ProjectWithNullEndDate_UpdatesProject()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Name = "Updated Ongoing Project",
            Description = "Updated description",
            StartDate = project.StartDate,
            EndDate = null
        };

        // Act
        var result = await _projectService.UpdateAsync(project.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Ongoing Project", result.Name);
        Assert.Null(result.EndDate);
    }

    [Fact]
    public async Task GetByIdAsync_ProjectWithZeroEmployees_ReturnsZeroEmployeeCount()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Empty Project",
            Description = "Project with no employees",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };
        Context.Projects.Add(project);
        await Context.SaveChangesAsync();

        // Act
        var result = await _projectService.GetByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.EmployeeCount);
        Assert.Equal("Empty Project", result.Name);
    }

    [Fact]
    public async Task GetEmployeesAsync_ProjectWithZeroEmployees_ReturnsEmpty()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Empty Project",
            Description = "Project with no employees",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };
        Context.Projects.Add(project);
        await Context.SaveChangesAsync();
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _projectService.GetEmployeesAsync(project.Id, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetEmployeesAsync_ExcludesSoftDeletedEmployees()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();
        var employee = await Context.Employees.FirstAsync();
        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _projectService.GetEmployeesAsync(project.Id, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(result.Items, e => e.Id == employee.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExcludesSoftDeletedEmployeesFromCount()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();
        var employee = await Context.Employees.FirstAsync();
        var initialCount = await Context.EmployeeProjects
            .CountAsync(ep => ep.ProjectId == project.Id && !ep.Employee.IsDeleted);
        
        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Act
        var result = await _projectService.GetByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.EmployeeCount < initialCount || initialCount == 0);
    }
}
