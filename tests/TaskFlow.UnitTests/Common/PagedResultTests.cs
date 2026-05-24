using FluentAssertions;
using TaskFlow.Application.Common;

namespace TaskFlow.UnitTests.Common;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(25, 10, 3)]
    [InlineData(20, 10, 2)]
    [InlineData(1, 10, 1)]
    public void Create_ShouldCalculateTotalPages(
        int totalCount,
        int pageSize,
        int expectedTotalPages)
    {
        var result = PagedResult<string>.Create([], page: 1, pageSize, totalCount);

        result.TotalPages.Should().Be(expectedTotalPages);
    }

    [Fact]
    public void Create_ShouldSetTotalPages_FromTotalCountAndPageSize()
    {
        var result = PagedResult<int>.Create([1, 2], page: 1, pageSize: 10, totalCount: 25);

        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(2);
    }
}
