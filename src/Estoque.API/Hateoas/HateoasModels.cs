namespace Estoque.API.Hateoas;

public sealed record Link(string Href, string Rel, string Method);

public sealed class Resource<T>
{
    public Resource(T data, IReadOnlyList<Link> links)
    {
        Data = data;
        Links = links;
    }

    public bool Success => true;

    public T Data { get; }

    public IReadOnlyList<Link> Links { get; }
}

public sealed class PagedResource<T>
{
    public PagedResource(
        IReadOnlyList<Resource<T>> items,
        int pageNumber,
        int pageSize,
        int totalItems,
        int totalPages,
        bool hasPrevious,
        bool hasNext,
        IReadOnlyList<Link> links)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = totalPages;
        HasPrevious = hasPrevious;
        HasNext = hasNext;
        Links = links;
    }

    public bool Success => true;

    public IReadOnlyList<Resource<T>> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalItems { get; }

    public int TotalPages { get; }

    public bool HasPrevious { get; }

    public bool HasNext { get; }

    public IReadOnlyList<Link> Links { get; }
}
