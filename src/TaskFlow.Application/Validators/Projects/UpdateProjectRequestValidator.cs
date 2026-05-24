using FluentValidation;
using TaskFlow.Application.DTOs.Projects;

namespace TaskFlow.Application.Validators.Projects;

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);
    }
}
