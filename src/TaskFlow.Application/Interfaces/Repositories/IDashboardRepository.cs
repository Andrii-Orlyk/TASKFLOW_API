using TaskFlow.Application.DTOs.Dashboard;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardSummaryResponse> GetSummaryForUserAsync(Guid userId, CancellationToken cancellationToken);
}
