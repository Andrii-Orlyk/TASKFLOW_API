using FluentValidation;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Validators.Tasks;

public sealed class TaskFilterRequestValidator : AbstractValidator<TaskFilterRequest>
{
    public TaskFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => x.Search is not null);

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue);
    }
}
