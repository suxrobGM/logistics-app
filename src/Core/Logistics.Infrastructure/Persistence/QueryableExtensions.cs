using Logistics.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
namespace Logistics.Infrastructure.Persistence;

public static class QueryableExtensions
{
    /// <summary>
    /// Maps one <see cref="ISpecification{T}"/> to an EF query.
    /// </summary>
    public static IQueryable<T> ApplySpecification<T>(
        this IQueryable<T> query,
        ISpecification<T> spec) where T : class
    {
        // filter
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        // includes
        foreach (var include in spec.Includes)
        {
            query = query.Include(include);
        }
        
        // sorting
        if (spec.Sort is { } sort)
        {
            if (PathHelper.IsSingleExactSegment<T>(sort.Property))
            {
                // fast path – one hop, perfect for EF.Property
                query = sort.Descending
                    ? query.OrderByDescending(e => EF.Property<object>(e, sort.Property))
                    : query.OrderBy(e => EF.Property<object>(e, sort.Property));
            }
            else
            {
                // nested or owned-type member: build expression once, cache afterward
                var selector = PathHelper.BuildSelector<T>(sort.Property);

                query = sort.Descending
                    ? query.OrderByDescending(selector)
                    : query.OrderBy (selector);
            }
        }

        // paging
        if (spec.Page is { Size: > 0 })
        {
            query = query
                .Skip((spec.Page.Value.Number - 1) * spec.Page.Value.Size)
                .Take(spec.Page.Value.Size);
        }

        return query;
    }
}
