using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TaskFlow.Application.Validation;

public sealed class RequestValidationService(IServiceProvider serviceProvider) : IRequestValidationService
{
    public async Task ValidateAsync(object request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);

        if (serviceProvider.GetService(validatorType) is not IValidator validator)
            return;

        var validationContextType = typeof(ValidationContext<>).MakeGenericType(requestType);
        var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, request)!;

        var result = await validator.ValidateAsync(validationContext, cancellationToken);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }
}
