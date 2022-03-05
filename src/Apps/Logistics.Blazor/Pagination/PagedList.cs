using System.Linq.Expressions;

namespace Logistics.Blazor.Pagination;

public class PagedList<T> : List<T>
{
    private readonly Dictionary<string, bool> cachedItems;
    private readonly Func<T, string> keySelector;
    private readonly bool allowCaching;

    public PagedList(
        int pageSize = 10,
        bool allowCaching = false,
        Expression<Func<T, string>> keySelectorExp = null!
        )
        : this(Array.Empty<T>(), 0, 1, pageSize, allowCaching, keySelectorExp)
    {
    }

    public PagedList(
        int totalItems,
        int pageSize = 10,
        bool allowCaching = false,
        Expression<Func<T, string>> keySelectorExp = null!
        )
        : this(Array.Empty<T>(), totalItems, 1, pageSize, allowCaching, keySelectorExp)
    {
    }

    public PagedList(
        IEnumerable<T> items, 
        int totalItems, 
        int currentPage = 1, 
        int pageSize = 10, 
        bool allowCaching = false, 
        Expression<Func<T, string>> keySelectorExp = null!)
    {
        if (currentPage <= 0)
            throw new ArgumentException("Current page should be positive integer number");

        if (totalItems < 0)
            throw new ArgumentException("Total items should be non-negative integer number");

        if (pageSize <= 0)
            throw new ArgumentException("Page size should be positive integer number");

        if (allowCaching && keySelectorExp != null)
        {
            keySelector = keySelectorExp.Compile();
        }
        else if (allowCaching && keySelectorExp == null)
        {
            throw new ArgumentException("Should be specified key selector when using cache mode");
        }
        else
        {
            keySelector = null!;
        }
        
        this.allowCaching = allowCaching;
        cachedItems = new Dictionary<string, bool>();
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        AddRange(items);
    }

    public int CurrentPage { get; private set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int PagesCount => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1 && PagesCount > 1;
    public bool HasNextPage => CurrentPage < PagesCount;

    public new void Add(T item)
    {
        if (HasInCache(item))
        {
            return;
        }
        else if (allowCaching)
        {
            AddToCache(item);
        }

        base.Add(item);
    }

    public new void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public new void Insert(int index, T item)
    {
        if (HasInCache(item))
        {
            return;
        }
        else if (allowCaching)
        {
            AddToCache(item);
        }

        base.Insert(index, item);
    }

    public new void InsertRange(int index, IEnumerable<T> items)
    {
        var itemsList = items.ToList();

        for (int i = index; i < itemsList.Count; i++)
        {
            Insert(i, itemsList[i]);
        }
    }

    public new bool Remove(T item)
    {
        RemoveFromCache(item);
        var removed = base.Remove(item);
        return removed;
    }

    public new void RemoveAt(int index)
    {
        T removedItem = this[index];
        RemoveFromCache(removedItem);
        base.RemoveAt(index);
    }

    public new void RemoveRange(int index, int count)
    {
        for (int i = index; i < count; i++)
        {
            RemoveAt(i);
        }
    }

    public new void RemoveAll(Predicate<T> match)
    {
        var removedItems = FindAll(match);
        foreach (var item in removedItems)
        {
            RemoveFromCache(item);
        }
        base.RemoveAll(match);
    }

    public new void Clear()
    {
        cachedItems.Clear();
        base.Clear();
    }

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

    private void AddToCache(T item)
    {
        if (!allowCaching)
            return;

        var key = keySelector(item);
        cachedItems.Add(key, true);
    }

    private void RemoveFromCache(T item)
    {
        var key = keySelector(item);
        cachedItems.Remove(key);
    }

    /// <summary>
    /// Checks whether item contains in the cache
    /// </summary>
    /// <param name="item">instance of item</param>
    /// <returns>True if item is in the cache, otherwise false</returns>
    private bool HasInCache(T item)
    {
        if (!allowCaching)
            return false;
   
        try
        {
            var key = keySelector(item);
            var _ = cachedItems[key];
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
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
