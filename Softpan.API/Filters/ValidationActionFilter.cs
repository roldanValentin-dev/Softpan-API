using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Softpan.API.Filters;

public class ValidationActionFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    
    public async Task OnActionExecutionAsync (ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Recorre todos los parámetros del método del controlador
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            var parameterName = parameter.Name;

            // Obtiene el valor del parámetro
            if (!context.ActionArguments.TryGetValue(parameterName, out var argument) || argument == null)
                continue;

            var argumentType = argument.GetType();

            // Busca el validador para este tipo en DI
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                // Ejecuta la validación
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                // Si hay errores, devuelve 400 con los detalles
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        message = e.ErrorMessage
                    });

                    context.Result = new BadRequestObjectResult(new
                    {
                        status = 400,
                        message = "Errores de validación",
                        errors
                    });
                    return; // Cortocircuita, NO ejecuta el controlador
                }
            }
        }

        // Si todo está OK, continúa al controlador
        await next();
    }
}

