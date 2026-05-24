using TaskFlow.Api.Models;
using TaskFlow.Application.Common;

namespace TaskFlow.Api.Extensions;

internal static class ApiErrorResponseFactory
{
    public const string ValidationFailedCode = "validation.failed";
    public const string InternalErrorCode = "internal.error";

    public static int GetStatusCode(string errorCode) =>
        errorCode switch
        {
            "auth.invalid_credentials" or "auth.unauthorized" => StatusCodes.Status401Unauthorized,
            "auth.email_exists" or "task.invalid_status_transition" => StatusCodes.Status409Conflict,
            "project.not_found" or "task.not_found" or "comment.not_found" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

    public static ApiErrorResponse FromError(Error error)
    {
        var statusCode = GetStatusCode(error.Code);
        return new ApiErrorResponse(statusCode, error.Code, error.Message, Array.Empty<string>());
    }

    public static ApiErrorResponse ValidationFailed(string message, IReadOnlyList<string> errors) =>
        new(StatusCodes.Status400BadRequest, ValidationFailedCode, message, errors);

    public static ApiErrorResponse InternalError(string message) =>
        new(StatusCodes.Status500InternalServerError, InternalErrorCode, message, Array.Empty<string>());
}
