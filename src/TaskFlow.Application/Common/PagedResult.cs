namespace TaskFlow.Application.Common;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }

    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = CalculateTotalPages(totalCount, pageSize)
        };
    }

    private static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
