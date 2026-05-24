using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Application.Services;

public sealed class DashboardService(
    IDashboardRepository dashboardRepository,
    ICurrentUserService currentUserService) : IDashboardService
{
    private const string UnauthorizedCode = "auth.unauthorized";
    private const string UnauthorizedMessage = "User is not authenticated.";

    public async Task<Result<DashboardSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is not Guid userId)
        {
            return Result<DashboardSummaryResponse>.Failure(
                Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var summary = await dashboardRepository.GetSummaryForUserAsync(userId, cancellationToken);
        return Result<DashboardSummaryResponse>.Success(summary);
    }
}
