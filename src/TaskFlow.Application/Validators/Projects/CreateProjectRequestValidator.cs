using FluentValidation;
using TaskFlow.Application.DTOs.Projects;

namespace TaskFlow.Application.Validators.Projects;

public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);
    }
}
