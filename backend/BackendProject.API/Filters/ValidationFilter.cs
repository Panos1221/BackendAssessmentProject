using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackendProject.API.Filters;

/// <summary>
/// Action filter that automatically validates request models using FluentValidation.
/// Sets the Id property from route parameters if present (for Update requests).
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Get route Id if present (for Update/Delete operations)
        Guid? routeId = null;
        if (context.RouteData.Values.TryGetValue("id", out var idValue) && 
            Guid.TryParse(idValue?.ToString(), out var parsedId))
        {
            routeId = parsedId;
        }

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null)
                continue;

            // Set Id property from route if the request has an Id property
            if (routeId.HasValue)
            {
                var idProperty = argument.GetType().GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(Guid))
                {
                    idProperty.SetValue(argument, routeId.Value);
                }
            }

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }
        }

        await next();
    }
}

/// <summary>
/// Extension methods for registering the ValidationFilter.
/// </summary>
public static class ValidationFilterExtensions
{
    /// <summary>
    /// Adds the ValidationFilter to all controllers.
    /// </summary>
    public static IMvcBuilder AddValidationFilter(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<ValidationFilter>();
        builder.AddMvcOptions(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
        return builder;
    }
}
