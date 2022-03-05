namespace Logistics.Blazor.Pagination;

public class PagedList<T> : List<T>
{
    public PagedList()
        : this(Array.Empty<T>(), 0)
    {
    }

    public PagedList(IEnumerable<T> items, int totalItems, int currentPage = 1, int pageSize = 10)
    {
        if (currentPage <= 0)
            throw new ArgumentException("Current page should be positive integer number");

        if (totalItems < 0)
            throw new ArgumentException("Total items should be non-negative integer number");

        if (pageSize <= 0)
            throw new ArgumentException("Page size should be positive integer number");

        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        AddRange(items);
    }

    public int CurrentPage { get; private set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1 && TotalPages > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public IEnumerable<T> NextPage()
    {
        if (!HasNextPage)
            return Array.Empty<T>();
        
        CurrentPage++;
        return GetPage(CurrentPage);
    }

    public IEnumerable<T> PreviousPage()
    {
        if (!HasPreviousPage)
            return Array.Empty<T>();

        CurrentPage--;
        return GetPage(CurrentPage);
    }

    public IEnumerable<T> GetPage(int pageNumber)
    {
        if (pageNumber <= 0)
            Array.Empty<T>();

        CurrentPage = pageNumber;
        return this.Skip((pageNumber - 1) * PageSize).Take(PageSize);
    }


    #region Static methods

    public static PagedList<T> Create(
        IEnumerable<T> source,
        int currentPage = 1,
        int pageSize = 10)
    {
        var totalItems = source.Count();
        var items = source.Skip((currentPage - 1) * pageSize)
            .Take(pageSize);
        return new PagedList<T>(items, totalItems, currentPage, pageSize);
    }

    public static PagedList<T> Create(
        IQueryable<T> query,
        int currentPage = 1,
        int pageSize = 10)
    {
        var totalItems = query.Count();
        var items = query.Skip((currentPage - 1) * pageSize)
            .Take(pageSize);
        return new PagedList<T>(items, totalItems, currentPage, pageSize);
    }

    public static PagedList<TOut> Create<TSource, TOut>(
        IQueryable<TSource> query,
        int currentPage,
        int pageSize,
        Func<TSource, TOut> converter)
    {
        var totalItems = query.Count();
        var items = query.Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(i => converter(i));
        return new PagedList<TOut>(items, totalItems, currentPage, pageSize);
    }

    #endregion
}

public static class PagedListExtensions
{
    public static PagedList<T> ToPagedList<T>(
        this IEnumerable<T> source,
        int currentPage = 1,
        int pageSize = 10)
    {
        return PagedList<T>.Create(source, currentPage, pageSize);
    }
}
