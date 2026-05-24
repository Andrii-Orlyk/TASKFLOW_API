using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Interfaces.Services;

public interface ITaskService
{
    Task<Result<PagedResult<TaskResponse>>> GetFilteredAsync(
        TaskFilterRequest filter,
        CancellationToken cancellationToken);

    Task<Result<TaskResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<TaskResponse>> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);

    Task<Result<TaskResponse>> UpdateAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken);

    Task<Result<TaskResponse>> UpdateStatusAsync(
        Guid id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
