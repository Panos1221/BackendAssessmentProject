using BackendProject.Application.DTOs;
using BackendProject.Domain.Interfaces;
using FluentValidation;

namespace BackendProject.Application.Validators;

/// <summary>
/// Validator for creating a new employee.
/// </summary>
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator(IUnitOfWork unitOfWork)
    {
        ApplyNameRules();
        ApplyHireDateRules();
        ApplyStatusRules();
        ApplyNotesRules();
        ApplyDepartmentRules(unitOfWork);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
            .MustAsync(async (email, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(email))
                    return true;

                var exists = await unitOfWork.Employees.AnyAsync(
                    e => e.Email.ToLower() == email.ToLower(),
                    cancellation);
                return !exists;
            })
            .WithMessage("An employee with this email address already exists.");
    }

    private void ApplyNameRules()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");
    }

    private void ApplyHireDateRules()
    {
        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Hire date cannot be in the future");
    }

    private void ApplyStatusRules()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid employee status");
    }

    private void ApplyNotesRules()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }

    private void ApplyDepartmentRules(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .MustAsync(async (departmentId, cancellation) =>
            {
                if (departmentId == Guid.Empty)
                    return true;

                return await unitOfWork.Departments.ExistsAsync(departmentId, cancellation);
            })
            .WithMessage("The specified department does not exist.");
    }
}

/// <summary>
/// Validator for updating an existing employee.
/// </summary>
public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeValidator(IUnitOfWork unitOfWork)
    {
        ApplyNameRules();
        ApplyHireDateRules();
        ApplyStatusRules();
        ApplyNotesRules();
        ApplyDepartmentRules(unitOfWork);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
            .MustAsync(async (request, email, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(email))
                    return true;

                var exists = await unitOfWork.Employees.AnyAsync(
                    e => e.Email.ToLower() == email.ToLower() && e.Id != request.Id,
                    cancellation);
                return !exists;
            })
            .WithMessage("An employee with this email address already exists.");
    }

    private void ApplyNameRules()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");
    }

    private void ApplyHireDateRules()
    {
        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Hire date cannot be in the future");
    }

    private void ApplyStatusRules()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid employee status");
    }

    private void ApplyNotesRules()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }

    private void ApplyDepartmentRules(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .MustAsync(async (departmentId, cancellation) =>
            {
                if (departmentId == Guid.Empty)
                    return true;

                return await unitOfWork.Departments.ExistsAsync(departmentId, cancellation);
            })
            .WithMessage("The specified department does not exist.");
    }
}
