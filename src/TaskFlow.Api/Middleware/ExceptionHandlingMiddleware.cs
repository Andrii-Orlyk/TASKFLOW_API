using System.Text.Json;
using FluentValidation;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException validationException)
        {
            var errors = validationException.Errors
                .Select(error => error.ErrorMessage)
                .ToList();

            await WriteErrorResponseAsync(
                context,
                ApiErrorResponseFactory.ValidationFailed("Validation failed", errors));
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(
                context,
                ApiErrorResponseFactory.InternalError("An unexpected error occurred."));
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, ApiErrorResponse response)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
