using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Dashboard;

namespace TaskFlow.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<Result<DashboardSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken);
}
