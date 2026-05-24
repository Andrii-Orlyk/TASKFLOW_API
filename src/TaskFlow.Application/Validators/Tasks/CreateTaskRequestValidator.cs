using FluentValidation;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Validators.Tasks;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Priority).ValidPriority();
        RuleFor(x => x.DueDate).ValidDueDate();
    }
}
