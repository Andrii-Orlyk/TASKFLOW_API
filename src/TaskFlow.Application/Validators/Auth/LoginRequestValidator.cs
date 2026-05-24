using FluentValidation;
using TaskFlow.Application.DTOs.Auth;

namespace TaskFlow.Application.Validators.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(100);
    }
}
