using FluentValidation;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Validators.Tasks;

internal static class TaskRequestValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.NotEmpty().MaximumLength(150);

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.MaximumLength(2000).When(x => x is not null);

    public static IRuleBuilderOptions<T, TaskPriority> ValidPriority<T>(
        this IRuleBuilder<T, TaskPriority> ruleBuilder) =>
        ruleBuilder.IsInEnum();

    public static IRuleBuilderOptions<T, DateTime?> ValidDueDate<T>(
        this IRuleBuilder<T, DateTime?> ruleBuilder) =>
        ruleBuilder
            .Must(dueDate => dueDate is null || dueDate.Value.Date >= DateTime.UtcNow.Date)
            .WithMessage("Due date cannot be in the past.");
}
