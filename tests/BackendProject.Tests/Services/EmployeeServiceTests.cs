using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Services;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Enums;
using BackendProject.Tests.TestFixture;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Services;

public class EmployeeServiceTests : ServiceTestBase
{
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _employeeService = new EmployeeService(Employees, Departments, Projects, SaveChanges);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Test Department",
            Description = "Test Description"
        };
        Context.Departments.Add(department);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Project Description",
            StartDate = DateTime.UtcNow
        };
        Context.Projects.Add(project);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Employee",
            Email = "test@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };
        Context.Employees.Add(employee);

        Context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmployees()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _employeeService.GetAllAsync(pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.NotEmpty(result.Items);
        Assert.Equal(pagination.PageNumber, result.PageNumber);
        Assert.Equal(pagination.PageSize, result.PageSize);
        var employee = result.Items.First();
        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.NotEmpty(employee.FirstName);
        Assert.NotEmpty(employee.LastName);
        Assert.NotEmpty(employee.Email);
        Assert.NotNull(employee.DepartmentName);
        Assert.NotEqual(Guid.Empty, employee.DepartmentId);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsEmployeeDetail()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();

        // Act
        var result = await _employeeService.GetByIdAsync(employee.Id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<EmployeeDetailResponse>(result);
        Assert.Equal(employee.FirstName, result.FirstName);
        Assert.Equal(employee.LastName, result.LastName);
        Assert.Equal(employee.Email, result.Email);
        Assert.NotNull(result.Projects);
        Assert.Equal(employee.DepartmentId, result.DepartmentId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEmployee_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_EmployeeWithProjects_ReturnsProjects()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var project = await Context.Projects.FirstAsync();
        
        // Assign employee to project
        await _employeeService.AssignToProjectAsync(employee.Id, project.Id);

        // Act
        var result = await _employeeService.GetByIdAsync(employee.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Projects);
        Assert.NotEmpty(result.Projects);
        Assert.Contains(result.Projects, p => p.Id == project.Id);
        Assert.Equal(project.Name, result.Projects.First(p => p.Id == project.Id).Name);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesEmployee()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "New",
            LastName = "Employee",
            Email = "new@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow,
            DepartmentId = department.Id
        };

        // Act
        var result = await _employeeService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Status, result.Status);
        Assert.Equal(request.HireDate, result.HireDate);
        Assert.Equal(request.DepartmentId, result.DepartmentId);
        Assert.Equal(department.Name, result.DepartmentName);
        
        // Verify it was actually saved to database
        var savedEmployee = await Context.Employees
            .FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.NotNull(savedEmployee);
        Assert.Equal(request.FirstName, savedEmployee.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEmployee_UpdatesEmployee()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = employee.Email,
            Status = EmployeeStatus.Active,
            HireDate = employee.HireDate,
            DepartmentId = employee.DepartmentId
        };

        // Act
        var result = await _employeeService.UpdateAsync(employee.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Name", result.LastName);
        Assert.Equal(employee.Email, result.Email);
        Assert.Equal(department.Name, result.DepartmentName);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingEmployee_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateEmployeeRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "test@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow,
            DepartmentId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task DeleteAsync_ExistingEmployee_SoftDeletesEmployee()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();

        // Act
        await _employeeService.DeleteAsync(employee.Id);

        // Assert
        var deletedEmployee = await Context.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == employee.Id);
        Assert.NotNull(deletedEmployee);
        Assert.True(deletedEmployee.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingEmployee_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SearchAsync_MatchingTerm_ReturnsResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _employeeService.SearchAsync("Test", pagination);

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
        var result = await _employeeService.SearchAsync("NonExistentName", pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task AssignToProjectAsync_ValidIds_AssignsEmployee()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var project = await Context.Projects.FirstAsync();

        // Act
        await _employeeService.AssignToProjectAsync(employee.Id, project.Id);

        // Assert
        var assignment = await Context.EmployeeProjects
            .FirstOrDefaultAsync(ep => ep.EmployeeId == employee.Id && ep.ProjectId == project.Id);
        Assert.NotNull(assignment);
    }

    [Fact]
    public async Task AssignToProjectAsync_NonExistingEmployee_ThrowsKeyNotFoundException()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.AssignToProjectAsync(Guid.NewGuid(), project.Id));
    }

    [Fact]
    public async Task AssignToProjectAsync_NonExistingProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.AssignToProjectAsync(employee.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task AssignToProjectAsync_AlreadyAssigned_IsIdempotent()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var project = await Context.Projects.FirstAsync();

        // First assignment
        await _employeeService.AssignToProjectAsync(employee.Id, project.Id);
        var firstAssignmentCount = await Context.EmployeeProjects
            .CountAsync(ep => ep.EmployeeId == employee.Id && ep.ProjectId == project.Id);

        // Act - assign again (should be idempotent)
        await _employeeService.AssignToProjectAsync(employee.Id, project.Id);

        // Assert - should still have only one assignment
        var finalAssignmentCount = await Context.EmployeeProjects
            .CountAsync(ep => ep.EmployeeId == employee.Id && ep.ProjectId == project.Id);
        Assert.Equal(1, firstAssignmentCount);
        Assert.Equal(1, finalAssignmentCount);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedEmployees()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Act
        var result = await _employeeService.GetAllAsync(new PaginationParams { PageNumber = 1, PageSize = 10 });

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(result.Items, e => e.Id == employee.Id);
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedEmployee_ThrowsKeyNotFoundException()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.GetByIdAsync(employee.Id));
    }

    [Fact]
    public async Task GetByIdAsync_EmployeeWithNoProjects_ReturnsEmptyProjectsList()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();

        // Act
        var result = await _employeeService.GetByIdAsync(employee.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Projects);
        Assert.Empty(result.Projects);
    }

    [Fact]
    public async Task RemoveFromProjectAsync_ExistingAssignment_RemovesAssignment()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var project = await Context.Projects.FirstAsync();

        // First assign
        await _employeeService.AssignToProjectAsync(employee.Id, project.Id);

        // Act
        await _employeeService.RemoveFromProjectAsync(employee.Id, project.Id);

        // Assert
        var assignment = await Context.EmployeeProjects
            .FirstOrDefaultAsync(ep => ep.EmployeeId == employee.Id && ep.ProjectId == project.Id);
        Assert.Null(assignment);
    }

    [Fact]
    public async Task RemoveFromProjectAsync_NonExistingEmployee_ThrowsKeyNotFoundException()
    {
        // Arrange
        var project = await Context.Projects.FirstAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.RemoveFromProjectAsync(Guid.NewGuid(), project.Id));
    }

    [Fact]
    public async Task RemoveFromProjectAsync_NonExistingAssignment_ThrowsKeyNotFoundException()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var project = await Context.Projects.FirstAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _employeeService.RemoveFromProjectAsync(employee.Id, project.Id));
    }
}
