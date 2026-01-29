using BackendProject.Application.DTOs;
using BackendProject.Application.Validators;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Validators;

public class EmployeeValidatorsTests : ValidatorTestBase
{
    private readonly CreateEmployeeValidator _createValidator;
    private readonly UpdateEmployeeValidator _updateValidator;

    public EmployeeValidatorsTests()
    {
        _createValidator = new CreateEmployeeValidator(Employees, Departments);
        _updateValidator = new UpdateEmployeeValidator(Employees, Departments);
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

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };
        Context.Employees.Add(employee);
        Context.SaveChanges();
    }

    #region CreateEmployeeValidator Tests

    [Fact]
    public async Task CreateValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateValidator_EmptyFirstName_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = string.Empty,
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName" && e.ErrorMessage == "First name is required");
    }

    [Fact]
    public async Task CreateValidator_FirstNameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = new string('A', 101),
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName" && e.ErrorMessage == "First name cannot exceed 100 characters");
    }

    [Fact]
    public async Task CreateValidator_EmptyLastName_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = string.Empty,
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LastName" && e.ErrorMessage == "Last name is required");
    }

    [Fact]
    public async Task CreateValidator_LastNameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = new string('A', 101),
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LastName" && e.ErrorMessage == "Last name cannot exceed 100 characters");
    }

    [Fact]
    public async Task CreateValidator_EmptyEmail_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = string.Empty,
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required");
    }

    [Fact]
    public async Task CreateValidator_InvalidEmailFormat_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "invalid-email",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format");
    }

    [Fact]
    public async Task CreateValidator_EmailExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = new string('A', 250) + "@example.com", // Exceeds 256
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email cannot exceed 256 characters");
    }

    [Fact]
    public async Task CreateValidator_DuplicateEmail_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "john.doe@example.com", // Same as seeded employee
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "An employee with this email address already exists.");
    }

    [Fact]
    public async Task CreateValidator_DuplicateEmailCaseInsensitive_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "JOHN.DOE@EXAMPLE.COM", // Different case
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "An employee with this email address already exists.");
    }

    [Fact]
    public async Task CreateValidator_InvalidStatus_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = (EmployeeStatus)999, // Invalid enum value
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status" && e.ErrorMessage == "Invalid employee status");
    }

    [Fact]
    public async Task CreateValidator_HireDateInFuture_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(1), // Future date
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HireDate" && e.ErrorMessage == "Hire date cannot be in the future");
    }

    [Fact]
    public async Task CreateValidator_EmptyDepartmentId_ShouldFail()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = Guid.Empty
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DepartmentId" && e.ErrorMessage == "Department is required");
    }

    [Fact]
    public async Task CreateValidator_NonExistentDepartment_ShouldFail()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = Guid.NewGuid() // Non-existent department
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DepartmentId" && e.ErrorMessage == "The specified department does not exist.");
    }

    [Fact]
    public async Task CreateValidator_NotesExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id,
            Notes = new string('A', 1001) // Exceeds max length
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Notes" && e.ErrorMessage == "Notes cannot exceed 1000 characters");
    }

    #endregion

    #region UpdateEmployeeValidator Tests

    [Fact]
    public async Task UpdateValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = employee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateValidator_SameEmailAsCurrentEmployee_ShouldPass()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = employee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = employee.Email, // Same email, same employee
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateValidator_DuplicateEmailDifferentEmployee_ShouldFail()
    {
        // Arrange
        var employee = await Context.Employees.FirstAsync();
        var anotherEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Another",
            LastName = "Employee",
            Email = "another@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = employee.DepartmentId
        };
        Context.Employees.Add(anotherEmployee);
        await Context.SaveChangesAsync();

        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = anotherEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = employee.Email, // Same email as first employee
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "An employee with this email address already exists.");
    }

    #endregion
}
