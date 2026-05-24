namespace TaskFlow.Application.Validation;

public interface IRequestValidationService
{
    Task ValidateAsync(object request, CancellationToken cancellationToken = default);
}
