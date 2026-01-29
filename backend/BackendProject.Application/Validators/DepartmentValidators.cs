using BackendProject.Application.DTOs;
using BackendProject.Domain.Entities;
using BackendProject.Domain.Interfaces;
using FluentValidation;

namespace BackendProject.Application.Validators;

/// <summary>
/// Validator for creating a new department.
/// </summary>
public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidator(IRepository<Department> departments)
    {
        ApplyNameRules();
        ApplyDescriptionRules();

        RuleFor(x => x.Name)
            .MustAsync(async (name, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                    return true;

                var exists = await departments.AnyAsync(
                    d => d.Name.ToLower() == name.ToLower(),
                    cancellation);
                return !exists;
            })
            .WithMessage("A department with this name already exists.");
    }

    private void ApplyNameRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(200).WithMessage("Department name cannot exceed 200 characters");
    }

    private void ApplyDescriptionRules()
    {
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}

/// <summary>
/// Validator for updating an existing department.
/// </summary>
public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentValidator(IRepository<Department> departments)
    {
        ApplyNameRules();
        ApplyDescriptionRules();

        RuleFor(x => x.Name)
            .MustAsync(async (request, name, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                    return true;

                var exists = await departments.AnyAsync(
                    d => d.Name.ToLower() == name.ToLower() && d.Id != request.Id,
                    cancellation);
                return !exists;
            })
            .WithMessage("A department with this name already exists.");
    }

    private void ApplyNameRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(200).WithMessage("Department name cannot exceed 200 characters");
    }

    private void ApplyDescriptionRules()
    {
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}
