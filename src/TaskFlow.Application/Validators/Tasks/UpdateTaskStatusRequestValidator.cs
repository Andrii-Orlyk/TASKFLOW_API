using FluentValidation;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Validators.Tasks;

public sealed class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
