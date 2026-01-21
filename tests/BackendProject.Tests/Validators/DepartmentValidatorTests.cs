using BackendProject.Application.DTOs;
using BackendProject.Application.Validators;
using BackendProject.Domain.Entities;
using BackendProject.Tests.TestFixture;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Validators;

/// <summary>
/// Unit tests for Department FluentValidation validators.
/// 
/// These tests verify INPUT VALIDATION rules (required fields, max length, format, uniqueness).
/// They run BEFORE service methods are called (via ValidationFilter).
/// 
/// IMPORTANT: These are NOT duplicates of DepartmentServiceTests.
/// - Validator Tests: Test validation rules in isolation (invalid input → validation errors)
/// - Service Tests: Test business logic with valid data (CRUD operations, relationships)
/// 
/// Example:
/// - Validator test: Empty name → validation error (never reaches service)
/// - Service test: Valid request → creates department in database
/// </summary>
public class DepartmentValidatorTests : ValidatorTestBase
{
    private readonly CreateDepartmentValidator _createValidator;
    private readonly UpdateDepartmentValidator _updateValidator;

    public DepartmentValidatorTests()
    {
        _createValidator = new CreateDepartmentValidator(UnitOfWork);
        _updateValidator = new UpdateDepartmentValidator(UnitOfWork);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var existingDepartment = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Existing Department",
            Description = "Existing Description"
        };
        Context.Departments.Add(existingDepartment);
        Context.SaveChanges();
    }

    #region CreateDepartmentValidator Tests

    [Fact]
    public async Task CreateValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "New Department",
            Description = "New Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateValidator_InvalidName_ShouldFail(string? name)
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = name!,
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name is required");
    }

    [Theory]
    [InlineData(200, true)]  // At max length
    [InlineData(201, false)] // Exceeds max length
    public async Task CreateValidator_NameLengthValidation_ShouldRespectMaxLength(int length, bool shouldPass)
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = new string('A', length),
            Description = "Description"
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name cannot exceed 200 characters");
        }
    }

    [Theory]
    [InlineData(1000, true)]  // At max length
    [InlineData(1001, false)] // Exceeds max length
    [InlineData(null, true)]  // Null is allowed
    public async Task CreateValidator_DescriptionLengthValidation_ShouldRespectMaxLength(int? length, bool shouldPass)
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "Valid Name",
            Description = length.HasValue ? new string('A', length.Value) : null
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 1000 characters");
        }
    }

    [Theory]
    [InlineData("Existing Department")]
    [InlineData("EXISTING DEPARTMENT")]
    public async Task CreateValidator_DuplicateName_ShouldFail(string name)
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = name,
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "A department with this name already exists.");
    }

    #endregion

    #region UpdateDepartmentValidator Tests

    [Fact]
    public async Task UpdateValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var existingDept = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDept.Id,
            Name = "Updated Department",
            Description = "Updated Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task UpdateValidator_InvalidName_ShouldFail(string? name)
    {
        // Arrange
        var existingDept = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDept.Id,
            Name = name!,
            Description = "Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name is required");
    }

    [Theory]
    [InlineData(200, true)]  // At max length
    [InlineData(201, false)] // Exceeds max length
    public async Task UpdateValidator_NameLengthValidation_ShouldRespectMaxLength(int length, bool shouldPass)
    {
        // Arrange
        var existingDept = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDept.Id,
            Name = new string('A', length),
            Description = "Description"
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name cannot exceed 200 characters");
        }
    }

    [Theory]
    [InlineData(1000, true)]  // At max length
    [InlineData(1001, false)] // Exceeds max length
    [InlineData(null, true)]  // Null is allowed
    public async Task UpdateValidator_DescriptionLengthValidation_ShouldRespectMaxLength(int? length, bool shouldPass)
    {
        // Arrange
        var existingDept = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDept.Id,
            Name = "Valid Name",
            Description = length.HasValue ? new string('A', length.Value) : null
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 1000 characters");
        }
    }

    [Fact]
    public async Task UpdateValidator_SameNameAsCurrentDepartment_ShouldPass()
    {
        // Arrange
        var existingDept = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDept.Id,
            Name = "Existing Department", // Same name as the department being updated
            Description = "Updated Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid); // Should pass because it's the same department
    }

    [Theory]
    [InlineData("Other Department")]
    [InlineData("OTHER DEPARTMENT")]
    public async Task UpdateValidator_DuplicateNameFromOtherDepartment_ShouldFail(string name)
    {
        // Arrange
        var existingDept = await Context.Departments.FirstAsync();
        var otherDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Other Department",
            Description = "Other"
        };
        Context.Departments.Add(otherDept);
        await Context.SaveChangesAsync();

        var request = new UpdateDepartmentRequest
        {
            Id = existingDept.Id,
            Name = name,
            Description = "Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "A department with this name already exists.");
    }

    #endregion
}
