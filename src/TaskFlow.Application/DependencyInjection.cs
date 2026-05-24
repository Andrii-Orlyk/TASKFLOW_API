using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Factories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Application.Policies;
using TaskFlow.Application.Services;
using TaskFlow.Application.Strategies;
using TaskFlow.Application.Validation;
using TaskFlow.Application.Validators.Auth;

namespace TaskFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddScoped<IRequestValidationService, RequestValidationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddSingleton<ITaskStatusPolicy, TaskStatusPolicy>();
        services.AddSingleton<ITaskPriorityStrategy, DeadlineAwarePriorityStrategy>();
        services.AddSingleton<ITaskFactory, Factories.TaskFactory>();

        return services;
    }
}
