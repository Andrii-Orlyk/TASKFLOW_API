using FluentAssertions;
using Moq;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Services;

namespace TaskFlow.UnitTests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _dashboardService = new DashboardService(
            _dashboardRepository.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task Dashboard_ShouldCountOnlyCurrentUserData()
    {
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var expectedSummary = new DashboardSummaryResponse(2, 5, 2, 1, 2, 1);

        _currentUserService.Setup(service => service.IsAuthenticated).Returns(true);
        _currentUserService.Setup(service => service.UserId).Returns(currentUserId);
        _dashboardRepository
            .Setup(repository => repository.GetSummaryForUserAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        var result = await _dashboardService.GetSummaryAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedSummary);
        _dashboardRepository.Verify(
            repository => repository.GetSummaryForUserAsync(currentUserId, It.IsAny<CancellationToken>()),
            Times.Once);
        _dashboardRepository.Verify(
            repository => repository.GetSummaryForUserAsync(otherUserId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldFail_WhenUserIsNotAuthenticated()
    {
        _currentUserService.Setup(service => service.IsAuthenticated).Returns(false);

        var result = await _dashboardService.GetSummaryAsync(CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("auth.unauthorized");
    }
}
