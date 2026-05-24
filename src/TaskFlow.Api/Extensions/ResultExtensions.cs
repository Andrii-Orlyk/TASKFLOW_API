using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common;

namespace TaskFlow.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return ToErrorActionResult(result.Error!);
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return ToErrorActionResult(result.Error!);
    }

    private static IActionResult ToErrorActionResult(Error error)
    {
        var response = ApiErrorResponseFactory.FromError(error);
        return new ObjectResult(response) { StatusCode = response.StatusCode };
    }
}
