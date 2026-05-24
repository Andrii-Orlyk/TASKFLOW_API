using FluentValidation;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Validators.Tasks;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Priority).ValidPriority();
        RuleFor(x => x.DueDate).ValidDueDate();
    }
}
