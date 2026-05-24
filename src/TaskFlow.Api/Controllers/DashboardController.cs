using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        (await dashboardService.GetSummaryAsync(cancellationToken)).ToActionResult();
}
