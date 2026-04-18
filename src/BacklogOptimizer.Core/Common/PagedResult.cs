namespace BacklogOptimizer.Core.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PagedResult(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        Items = items.ToList().AsReadOnly();
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}