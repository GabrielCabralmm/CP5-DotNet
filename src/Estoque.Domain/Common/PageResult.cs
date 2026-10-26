namespace Estoque.Domain.Common;

public sealed record PageRequest
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    private PageRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;

    public static PageRequest Create(int pageNumber, int pageSize)
    {
        var page = pageNumber < 1 ? 1 : pageNumber;
        var size = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return new PageRequest(page, size);
    }
}

public sealed class PageResult<T>
{
    public PageResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalItems)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalItems { get; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;

    public PageResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new(Items.Select(map).ToList(), PageNumber, PageSize, TotalItems);
}
