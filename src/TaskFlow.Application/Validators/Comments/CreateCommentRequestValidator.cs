using FluentValidation;
using TaskFlow.Application.DTOs.Comments;

namespace TaskFlow.Application.Validators.Comments;

public sealed class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
