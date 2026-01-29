using BackendProject.Application.DTOs;
using BackendProject.Application.Validators;
using BackendProject.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Validators;

public class DepartmentValidatorsTests : ValidatorTestBase
{
    private readonly CreateDepartmentValidator _createValidator;
    private readonly UpdateDepartmentValidator _updateValidator;

    public DepartmentValidatorsTests()
    {
        _createValidator = new CreateDepartmentValidator(Departments);
        _updateValidator = new UpdateDepartmentValidator(Departments);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Existing Department",
            Description = "Existing Description"
        };
        Context.Departments.Add(department);
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
    }

    [Fact]
    public async Task CreateValidator_EmptyName_ShouldFail()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = string.Empty,
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name is required");
    }

    [Fact]
    public async Task CreateValidator_NullName_ShouldFail()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = null!,
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name is required");
    }

    [Fact]
    public async Task CreateValidator_WhitespaceName_ShouldFail()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "   ",
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name is required");
    }

    [Fact]
    public async Task CreateValidator_NameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = new string('A', 201), // 201 characters
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name cannot exceed 200 characters");
    }

    [Fact]
    public async Task CreateValidator_NameAtMaxLength_ShouldPass()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = new string('A', 200), // Exactly 200 characters
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateValidator_DescriptionExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "Valid Name",
            Description = new string('A', 1001) // 1001 characters
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 1000 characters");
    }

    [Fact]
    public async Task CreateValidator_DescriptionAtMaxLength_ShouldPass()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "Valid Name",
            Description = new string('A', 1000) // Exactly 1000 characters
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task CreateValidator_NullDescription_ShouldPass()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "Valid Name",
            Description = null
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task CreateValidator_DuplicateName_ShouldFail()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "Existing Department", // Same as seeded data
            Description = "Description"
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "A department with this name already exists.");
    }

    [Fact]
    public async Task CreateValidator_DuplicateNameCaseInsensitive_ShouldFail()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "EXISTING DEPARTMENT", // Different case
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
        var existingDepartment = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDepartment.Id,
            Name = "Updated Department",
            Description = "Updated Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateValidator_EmptyName_ShouldFail()
    {
        // Arrange
        var existingDepartment = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDepartment.Id,
            Name = string.Empty,
            Description = "Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name is required");
    }

    [Fact]
    public async Task UpdateValidator_NameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var existingDepartment = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDepartment.Id,
            Name = new string('A', 201),
            Description = "Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Department name cannot exceed 200 characters");
    }

    [Fact]
    public async Task UpdateValidator_DescriptionExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var existingDepartment = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDepartment.Id,
            Name = "Valid Name",
            Description = new string('A', 1001)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 1000 characters");
    }

    [Fact]
    public async Task UpdateValidator_SameNameAsCurrentDepartment_ShouldPass()
    {
        // Arrange
        var existingDepartment = await Context.Departments.FirstAsync();
        var request = new UpdateDepartmentRequest
        {
            Id = existingDepartment.Id,
            Name = "Existing Department", // Same name, same department
            Description = "Updated Description"
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task UpdateValidator_DuplicateNameDifferentDepartment_ShouldFail()
    {
        // Arrange
        var existingDepartment = await Context.Departments.FirstAsync();
        var anotherDepartment = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Another Department",
            Description = "Another Description"
        };
        Context.Departments.Add(anotherDepartment);
        await Context.SaveChangesAsync();

        var request = new UpdateDepartmentRequest
        {
            Id = anotherDepartment.Id,
            Name = "Existing Department", // Same name as first department
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
