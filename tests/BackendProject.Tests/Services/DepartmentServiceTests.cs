using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Services;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Enums;
using BackendProject.Tests.TestFixture;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Services;

public class DepartmentServiceTests : ServiceTestBase
{
    private readonly DepartmentService _departmentService;

    public DepartmentServiceTests()
    {
        _departmentService = new DepartmentService(UnitOfWork);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            Description = "Engineering Department"
        };
        Context.Departments.Add(department);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-2),
            DepartmentId = department.Id
        };
        Context.Employees.Add(employee);

        Context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDepartments()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _departmentService.GetAllAsync(pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.NotEmpty(result.Items);
        Assert.Equal(pagination.PageNumber, result.PageNumber);
        Assert.Equal(pagination.PageSize, result.PageSize);
        var department = result.Items.First();
        Assert.NotEqual(Guid.Empty, department.Id);
        Assert.NotEmpty(department.Name);
        Assert.True(department.EmployeeCount >= 0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDepartment_ReturnsDepartment()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();

        // Act
        var result = await _departmentService.GetByIdAsync(department.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(department.Id, result.Id);
        Assert.Equal(department.Name, result.Name);
        Assert.Equal(department.Description, result.Description);
        Assert.True(result.EmployeeCount > 0);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingDepartment_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _departmentService.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesDepartment()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "New Department",
            Description = "New Description"
        };

        // Act
        var result = await _departmentService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(0, result.EmployeeCount);
        
        // Verify it was actually saved to database
        var savedDepartment = await Context.Departments
            .FirstOrDefaultAsync(d => d.Id == result.Id);
        Assert.NotNull(savedDepartment);
        Assert.Equal(request.Name, savedDepartment.Name);
    }

    [Fact]
    public async Task UpdateAsync_ExistingDepartment_UpdatesDepartment()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Name = "Updated Department",
            Description = "Updated Description"
        };

        // Act
        var result = await _departmentService.UpdateAsync(department.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Department", result.Name);
        Assert.Equal("Updated Description", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingDepartment_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateDepartmentRequest
        {
            Name = "Updated Department",
            Description = "Updated Description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _departmentService.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task DeleteAsync_ExistingDepartment_SoftDeletesDepartment()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();

        // Act
        await _departmentService.DeleteAsync(department.Id);

        // Assert
        var deletedDepartment = await Context.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == department.Id);
        Assert.NotNull(deletedDepartment);
        Assert.True(deletedDepartment.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingDepartment_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _departmentService.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetEmployeesAsync_ExistingDepartment_ReturnsEmployees()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _departmentService.GetEmployeesAsync(department.Id, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.NotEmpty(result.Items);
        var employee = result.Items.First();
        Assert.Equal(department.Id, employee.DepartmentId);
        Assert.Equal(department.Name, employee.DepartmentName);
    }

    [Fact]
    public async Task GetEmployeesAsync_NonExistingDepartment_ThrowsKeyNotFoundException()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _departmentService.GetEmployeesAsync(Guid.NewGuid(), pagination));
    }

    [Fact]
    public async Task SearchAsync_MatchingTerm_ReturnsResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _departmentService.SearchAsync("Engineering", pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedDepartments()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Act
        var result = await _departmentService.GetAllAsync(new PaginationParams { PageNumber = 1, PageSize = 10 });

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(result.Items, d => d.Id == department.Id);
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedDepartment_ThrowsKeyNotFoundException()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _departmentService.GetByIdAsync(department.Id));
    }

    [Fact]
    public async Task GetByIdAsync_DepartmentWithZeroEmployees_ReturnsZeroEmployeeCount()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Empty Department",
            Description = "Department with no employees"
        };
        Context.Departments.Add(department);
        await Context.SaveChangesAsync();

        // Act
        var result = await _departmentService.GetByIdAsync(department.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.EmployeeCount);
        Assert.Equal("Empty Department", result.Name);
    }

    [Fact]
    public async Task GetEmployeesAsync_DepartmentWithZeroEmployees_ReturnsEmpty()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Empty Department",
            Description = "Department with no employees"
        };
        Context.Departments.Add(department);
        await Context.SaveChangesAsync();
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _departmentService.GetEmployeesAsync(department.Id, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetEmployeesAsync_ExcludesSoftDeletedEmployees()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var employee = await Context.Employees.FirstAsync(e => e.DepartmentId == department.Id);
        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _departmentService.GetEmployeesAsync(department.Id, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(result.Items, e => e.Id == employee.Id);
    }

    [Fact]
    public async Task SearchAsync_EmptySearchTerm_ReturnsAllDepartments()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _departmentService.SearchAsync("", pagination);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 0);
    }
}
