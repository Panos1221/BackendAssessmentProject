using BackendProject.Application.DTOs;
using BackendProject.Domain.Interfaces;
using FluentValidation;

namespace BackendProject.Application.Validators;

/// <summary>
/// Validator for creating a new project.
/// </summary>
public class CreateProjectValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectValidator(IUnitOfWork unitOfWork)
    {
        ApplyDescriptionRules();
        ApplyDateRules();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters")
            .MustAsync(async (name, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                    return true;

                var exists = await unitOfWork.Projects.AnyAsync(
                    p => p.Name.ToLower() == name.ToLower(),
                    cancellation);
                return !exists;
            })
            .WithMessage("A project with this name already exists.");
    }

    private void ApplyDescriptionRules()
    {
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }

    private void ApplyDateRules()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}

/// <summary>
/// Validator for updating an existing project.
/// </summary>
public class UpdateProjectValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectValidator(IUnitOfWork unitOfWork)
    {
        ApplyDescriptionRules();
        ApplyDateRules();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters")
            .MustAsync(async (request, name, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                    return true;

                var exists = await unitOfWork.Projects.AnyAsync(
                    p => p.Name.ToLower() == name.ToLower() && p.Id != request.Id,
                    cancellation);
                return !exists;
            })
            .WithMessage("A project with this name already exists.");
    }

    private void ApplyDescriptionRules()
    {
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }

    private void ApplyDateRules()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}
