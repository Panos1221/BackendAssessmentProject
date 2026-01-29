using BackendProject.Application.DTOs;
using BackendProject.Application.Validators;
using BackendProject.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BackendProject.Tests.Validators;

public class ProjectValidatorsTests : ValidatorTestBase
{
    private readonly CreateProjectValidator _createValidator;
    private readonly UpdateProjectValidator _updateValidator;

    public ProjectValidatorsTests()
    {
        _createValidator = new CreateProjectValidator(Projects);
        _updateValidator = new UpdateProjectValidator(Projects);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Existing Project",
            Description = "Existing Description",
            StartDate = DateTime.UtcNow.AddMonths(-6),
            EndDate = DateTime.UtcNow.AddMonths(6)
        };
        Context.Projects.Add(project);
        Context.SaveChanges();
    }

    #region CreateProjectValidator Tests

    [Fact]
    public async Task CreateValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "New Project",
            Description = "New Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
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
        var request = new CreateProjectRequest
        {
            Name = string.Empty,
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

    [Fact]
    public async Task CreateValidator_NameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = new string('A', 201),
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Project name cannot exceed 200 characters");
    }

    [Fact]
    public async Task CreateValidator_DescriptionExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = new string('A', 2001),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 2000 characters");
    }

    [Fact]
    public async Task CreateValidator_NullDescription_ShouldPass()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = null,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Description");
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

    [Fact]
    public async Task CreateValidator_EndDateBeforeStartDate_ShouldFail()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1) // End date before start date
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public async Task CreateValidator_EndDateEqualToStartDate_ShouldFail()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = "Description",
            StartDate = startDate,
            EndDate = startDate // End date equal to start date
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public async Task CreateValidator_NullEndDate_ShouldPass()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = null
        };

        // Act
        var result = await _createValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateValidator_DuplicateName_ShouldFail()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Existing Project", // Same as seeded data
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

    [Fact]
    public async Task CreateValidator_DuplicateNameCaseInsensitive_ShouldFail()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "EXISTING PROJECT", // Different case
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

    [Fact]
    public async Task UpdateValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
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
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = string.Empty,
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

    [Fact]
    public async Task UpdateValidator_NameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = new string('A', 201),
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Project name cannot exceed 200 characters");
    }

    [Fact]
    public async Task UpdateValidator_DescriptionExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Valid Name",
            Description = new string('A', 2001),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 2000 characters");
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

    [Fact]
    public async Task UpdateValidator_EndDateBeforeStartDate_ShouldFail()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Valid Name",
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public async Task UpdateValidator_NullEndDate_ShouldPass()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Valid Name",
            Description = "Description",
            StartDate = DateTime.UtcNow,
            EndDate = null
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateValidator_SameNameAsCurrentProject_ShouldPass()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var request = new UpdateProjectRequest
        {
            Id = existingProject.Id,
            Name = "Existing Project", // Same name, same project
            Description = "Updated Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateValidator_DuplicateNameDifferentProject_ShouldFail()
    {
        // Arrange
        var existingProject = await Context.Projects.FirstAsync();
        var anotherProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Another Project",
            Description = "Another Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };
        Context.Projects.Add(anotherProject);
        await Context.SaveChangesAsync();

        var request = new UpdateProjectRequest
        {
            Id = anotherProject.Id,
            Name = "Existing Project", // Same name as first project
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
