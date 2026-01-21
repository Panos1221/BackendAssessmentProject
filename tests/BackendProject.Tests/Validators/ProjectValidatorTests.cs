using BackendProject.Application.DTOs;
using BackendProject.Application.Validators;
using BackendProject.Domain.Entities;
using BackendProject.Tests.TestFixture;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Validators;

/// <summary>
/// Unit tests for Project FluentValidation validators.
/// 
/// These tests verify INPUT VALIDATION rules (required fields, max length, date validation, uniqueness).
/// They run BEFORE service methods are called (via ValidationFilter).
/// 
/// IMPORTANT: These are NOT duplicates of ProjectServiceTests.
/// - Validator Tests: Test validation rules in isolation (invalid input → validation errors)
/// - Service Tests: Test business logic with valid data (CRUD operations, employee assignments)
/// 
/// Example:
/// - Validator test: End date before start date → validation error (never reaches service)
/// - Service test: Valid request → creates project and manages employee assignments
/// </summary>
public class ProjectValidatorTests : ValidatorTestBase
{
    private readonly CreateProjectValidator _createValidator;
    private readonly UpdateProjectValidator _updateValidator;

    public ProjectValidatorTests()
    {
        _createValidator = new CreateProjectValidator(UnitOfWork);
        _updateValidator = new UpdateProjectValidator(UnitOfWork);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var existingProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Existing Project",
            Description = "Existing Description",
            StartDate = DateTime.UtcNow.AddMonths(-6),
            EndDate = DateTime.UtcNow.AddMonths(6)
        };
        Context.Projects.Add(existingProject);
        Context.SaveChanges();
    }

    #region CreateProjectValidator Tests

    [Theory]
    [InlineData(false)] // With end date
    [InlineData(true)]  // Without end date (null)
    public async Task CreateValidator_ValidRequest_ShouldPass(bool nullEndDate)
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "New Project",
            Description = "New Description",
            StartDate = DateTime.UtcNow,
            EndDate = nullEndDate ? null : DateTime.UtcNow.AddMonths(6)
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
        var request = new CreateProjectRequest
        {
            Name = name!,
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Project name is required");
    }

    [Theory]
    [InlineData(200, true)]  // At max length
    [InlineData(201, false)] // Exceeds max length
    public async Task CreateValidator_NameLengthValidation_ShouldRespectMaxLength(int length, bool shouldPass)
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = new string('A', length),
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Project name cannot exceed 200 characters");
        }
    }

    [Theory]
    [InlineData(2000, true)]  // At max length
    [InlineData(2001, false)] // Exceeds max length
    [InlineData(null, true)]  // Null is allowed
    public async Task CreateValidator_DescriptionLengthValidation_ShouldRespectMaxLength(int? length, bool shouldPass)
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = length.HasValue ? new string('A', length.Value) : null,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 2000 characters");
        }
    }

    [Fact]
    public async Task CreateValidator_EmptyStartDate_ShouldFail()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = "Description",
            StartDate = default(DateTime),
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate" && e.ErrorMessage == "Start date is required");
    }

    [Theory]
    [InlineData(false)] // End date before start date
    [InlineData(true)]  // End date equal to start date
    public async Task CreateValidator_InvalidEndDate_ShouldFail(bool equalToStart)
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = "Description",
            StartDate = startDate,
            EndDate = equalToStart ? startDate : startDate.AddDays(-1)
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public async Task CreateValidator_EndDateAfterStartDate_ShouldPass()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = "Description",
            StartDate = startDate,
            EndDate = startDate.AddDays(1) // End date after start date
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Existing Project")]
    [InlineData("EXISTING PROJECT")]
    public async Task CreateValidator_DuplicateName_ShouldFail(string name)
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = name,
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "A project with this name already exists.");
    }

    #endregion

    #region UpdateProjectValidator Tests

    [Theory]
    [InlineData(false)] // With end date
    [InlineData(true)]  // Without end date (null)
    public async Task UpdateValidator_ValidRequest_ShouldPass(bool nullEndDate)
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow,
            EndDate = nullEndDate ? null : DateTime.UtcNow.AddMonths(6)
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
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = name!,
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Project name is required");
    }

    [Theory]
    [InlineData(200, true)]  // At max length
    [InlineData(201, false)] // Exceeds max length
    public async Task UpdateValidator_NameLengthValidation_ShouldRespectMaxLength(int length, bool shouldPass)
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = new string('A', length),
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Project name cannot exceed 200 characters");
        }
    }

    [Theory]
    [InlineData(2000, true)]  // At max length
    [InlineData(2001, false)] // Exceeds max length
    [InlineData(null, true)]  // Null is allowed
    public async Task UpdateValidator_DescriptionLengthValidation_ShouldRespectMaxLength(int? length, bool shouldPass)
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Valid Name",
            Description = length.HasValue ? new string('A', length.Value) : null,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
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
            Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 2000 characters");
        }
    }

    [Fact]
    public async Task UpdateValidator_EmptyStartDate_ShouldFail()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Valid Name",
            Description = "Description",
            StartDate = default(DateTime),
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate" && e.ErrorMessage == "Start date is required");
    }

    [Theory]
    [InlineData(false)] // End date before start date
    [InlineData(true)]  // End date equal to start date
    public async Task UpdateValidator_InvalidEndDate_ShouldFail(bool equalToStart)
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var startDate = DateTime.UtcNow;
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Valid Name",
            Description = "Description",
            StartDate = startDate,
            EndDate = equalToStart ? startDate : startDate.AddDays(-1)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public async Task UpdateValidator_SameNameAsCurrentProject_ShouldPass()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Existing Project", // Same name as the project being updated
            Description = "Updated Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid); // Should pass because it's the same project
    }

    [Theory]
    [InlineData("Other Project")]
    [InlineData("OTHER PROJECT")]
    public async Task UpdateValidator_DuplicateNameFromOtherProject_ShouldFail(string name)
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var otherProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Other Project",
            Description = "Other",
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow.AddMonths(3)
        };
        Context.Projects.Add(otherProject);
        await Context.SaveChangesAsync();

        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = name,
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "A project with this name already exists.");
    }

    #endregion
}
