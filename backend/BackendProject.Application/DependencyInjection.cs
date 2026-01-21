using BackendProject.Application.Interfaces;
using BackendProject.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BackendProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IProjectService, ProjectService>();

        return services;
    }
}
