using Microsoft.AspNetCore.Mvc.Filters;
using TaskFlow.Application.Validation;

namespace TaskFlow.Api.Filters;

public sealed class ValidationActionFilter(IRequestValidationService requestValidationService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            await requestValidationService.ValidateAsync(argument, context.HttpContext.RequestAborted);
        }

        await next();
    }
}
