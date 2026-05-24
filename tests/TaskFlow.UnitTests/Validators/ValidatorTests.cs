using FluentAssertions;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Validators.Auth;
using TaskFlow.Application.Validators.Comments;
using TaskFlow.Application.Validators.Tasks;
using TaskFlow.Domain.Enums;

namespace TaskFlow.UnitTests.Validators;

public class ValidatorTests
{
    [Fact]
    public void RegisterRequestValidator_ShouldFail_WhenEmailIsInvalid()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("not-an-email", "password123", "John", "Doe");

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Email));
    }

    [Fact]
    public void CreateTaskRequestValidator_ShouldFail_WhenDueDateIsInPast()
    {
        var validator = new CreateTaskRequestValidator();
        var request = new CreateTaskRequest(
            Guid.NewGuid(),
            "Title",
            null,
            TaskPriority.Medium,
            DateTime.UtcNow.Date.AddDays(-1));

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateTaskRequest.DueDate));
    }

    [Fact]
    public void CreateCommentRequestValidator_ShouldFail_WhenContentIsEmpty()
    {
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest(string.Empty);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateCommentRequest.Content));
    }
}
