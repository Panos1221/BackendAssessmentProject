using BackendProject.Application.DTOs;
using BackendProject.Application.Validators;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Enums;
using BackendProject.Tests.TestFixture;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Validators;

/// <summary>
/// Unit tests for Employee FluentValidation validators.
/// 
/// These tests verify INPUT VALIDATION rules (required fields, max length, email format, 
/// date validation, uniqueness, department existence).
/// They run BEFORE service methods are called (via ValidationFilter).
/// 
/// IMPORTANT: These are NOT duplicates of EmployeeServiceTests.
/// - Validator Tests: Test validation rules in isolation (invalid input → validation errors)
/// - Service Tests: Test business logic with valid data (CRUD operations, project assignments)
/// 
/// Example:
/// - Validator test: Invalid email format → validation error (never reaches service)
/// - Service test: Valid request → creates employee and assigns to projects
/// </summary>
public class EmployeeValidatorTests : ValidatorTestBase
{
    private readonly CreateEmployeeValidator _createValidator;
    private readonly UpdateEmployeeValidator _updateValidator;

    public EmployeeValidatorTests()
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

        var existingEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "Employee",
            Email = "existing@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };
        Context.Employees.Add(existingEmployee);

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
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("FirstName", "", "First name is required")]
    [InlineData("FirstName", "   ", "First name is required")]
    [InlineData("LastName", "", "Last name is required")]
    [InlineData("LastName", "   ", "Last name is required")]
    public async Task CreateValidator_RequiredNameFields_ShouldFail(string propertyName, string value, string expectedMessage)
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = propertyName == "FirstName" ? value : "John",
            LastName = propertyName == "LastName" ? value : "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == propertyName && e.ErrorMessage == expectedMessage);
    }

    [Theory]
    [InlineData("FirstName", 100, true)]
    [InlineData("FirstName", 101, false)]
    [InlineData("LastName", 100, true)]
    [InlineData("LastName", 101, false)]
    public async Task CreateValidator_NameFieldsMaxLength_ShouldRespectLimit(string propertyName, int length, bool shouldPass)
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var nameValue = new string('A', length);
        var request = new CreateEmployeeRequest
        {
            FirstName = propertyName == "FirstName" ? nameValue : "John",
            LastName = propertyName == "LastName" ? nameValue : "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        if (shouldPass)
        {
            Assert.True(result.IsValid);
        }
        else
        {
            Assert.False(result.IsValid);
            var expectedMessage = propertyName == "FirstName" 
                ? "First name cannot exceed 100 characters" 
                : "Last name cannot exceed 100 characters";
            Assert.Contains(result.Errors, e => e.PropertyName == propertyName && e.ErrorMessage == expectedMessage);
        }
    }

    [Theory]
    [InlineData("", "Email is required")]
    [InlineData("invalid-email", "Invalid email format")]
    public async Task CreateValidator_InvalidEmail_ShouldFail(string email, string expectedMessage)
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public async Task CreateValidator_EmailExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = new string('a', 250) + "@example.com", // Exceeds 256 characters
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email cannot exceed 256 characters");
    }

    [Theory]
    [InlineData("existing@example.com")]
    [InlineData("EXISTING@EXAMPLE.COM")]
    public async Task CreateValidator_DuplicateEmail_ShouldFail(string email)
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "An employee with this email address already exists.");
    }

    [Theory]
    [InlineData(true, "Hire date cannot be in the future")]  // Future date
    [InlineData(false, "Hire date is required")]  // Default/empty date
    public async Task CreateValidator_InvalidHireDate_ShouldFail(bool isFuture, string expectedMessage)
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = isFuture ? DateTime.UtcNow.AddDays(1) : default(DateTime),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HireDate" && e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public async Task CreateValidator_HireDateToday_ShouldPass()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var hireDate = DateTime.UtcNow.AddSeconds(-1); // Slightly in the past to account for timing
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = hireDate,
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateValidator_InvalidStatus_ShouldFail()
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = (EmployeeStatus)999, // Invalid enum value
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status" && e.ErrorMessage == "Invalid employee status");
    }

    [Fact]
    public async Task CreateValidator_EmptyDepartmentId_ShouldFail()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = Guid.Empty
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DepartmentId" && e.ErrorMessage == "Department is required");
    }

    [Fact]
    public async Task CreateValidator_NonExistentDepartmentId_ShouldFail()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = Guid.NewGuid() // Non-existent department
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DepartmentId" && e.ErrorMessage == "The specified department does not exist.");
    }

    [Theory]
    [InlineData(1000, true)]  // At max length
    [InlineData(1001, false)] // Exceeds max length
    [InlineData(null, true)]  // Null is allowed
    public async Task CreateValidator_NotesLengthValidation_ShouldRespectMaxLength(int? length, bool shouldPass)
    {
        // Arrange
        var department = await Context.Departments.FirstAsync();
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id,
            Notes = length.HasValue ? new string('A', length.Value) : null
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        if (shouldPass)
        {
            Assert.True(result.IsValid);
        }
        else
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Notes" && e.ErrorMessage == "Notes cannot exceed 1000 characters");
        }
    }

    #endregion

    #region UpdateEmployeeValidator Tests

    [Fact]
    public async Task UpdateValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("FirstName", "", "First name is required")]
    [InlineData("FirstName", "   ", "First name is required")]
    [InlineData("LastName", "", "Last name is required")]
    [InlineData("LastName", "   ", "Last name is required")]
    public async Task UpdateValidator_RequiredNameFields_ShouldFail(string propertyName, string value, string expectedMessage)
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = propertyName == "FirstName" ? value : "John",
            LastName = propertyName == "LastName" ? value : "Doe",
            Email = "updated@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == propertyName && e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public async Task UpdateValidator_SameEmailAsCurrentEmployee_ShouldPass()
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "existing@example.com", // Same email as the employee being updated
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid); // Should pass because it's the same employee
    }

    [Theory]
    [InlineData("other@example.com")]
    [InlineData("OTHER@EXAMPLE.COM")]
    public async Task UpdateValidator_DuplicateEmailFromOtherEmployee_ShouldFail(string email)
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var otherEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Other",
            LastName = "Employee",
            Email = "other@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DepartmentId = department.Id
        };
        Context.Employees.Add(otherEmployee);
        await Context.SaveChangesAsync();

        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = email,
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "An employee with this email address already exists.");
    }

    [Fact]
    public async Task UpdateValidator_InvalidEmailFormat_ShouldFail()
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "invalid-email",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format");
    }

    [Fact]
    public async Task UpdateValidator_HireDateInFuture_ShouldFail()
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(1), // Future date
            DepartmentId = department.Id
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HireDate" && e.ErrorMessage == "Hire date cannot be in the future");
    }

    [Fact]
    public async Task UpdateValidator_NonExistentDepartmentId_ShouldFail()
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = Guid.NewGuid() // Non-existent department
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DepartmentId" && e.ErrorMessage == "The specified department does not exist.");
    }

    [Theory]
    [InlineData(1000, true)]  // At max length
    [InlineData(1001, false)] // Exceeds max length
    [InlineData(null, true)]  // Null is allowed
    public async Task UpdateValidator_NotesLengthValidation_ShouldRespectMaxLength(int? length, bool shouldPass)
    {
        // Arrange
        var existingEmployee = await Context.Employees.FirstAsync();
        var department = await Context.Departments.FirstAsync();
        var request = new UpdateEmployeeRequest
        {
            Id = existingEmployee.Id,
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Status = EmployeeStatus.Active,
            HireDate = DateTime.UtcNow.AddDays(-1),
            DepartmentId = department.Id,
            Notes = length.HasValue ? new string('A', length.Value) : null
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        if (shouldPass)
        {
            Assert.True(result.IsValid);
        }
        else
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Notes" && e.ErrorMessage == "Notes cannot exceed 1000 characters");
        }
    }

    #endregion
}
