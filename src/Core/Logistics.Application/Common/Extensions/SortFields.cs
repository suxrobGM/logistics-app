using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;
using Logistics.Shared.Models;

namespace Logistics.Application.Common.Extensions;

/// <summary>
/// The sort keys a list accepts. A key is the camelCase DTO path its column shows (<c>customer.name</c>),
/// mapped here to the query expression, so renaming an entity property never changes the API.
/// </summary>
public sealed class SortFields<T>(string? defaultOrderBy = null)
{
    private readonly Dictionary<string, Func<IQueryable<T>, bool, IOrderedQueryable<T>>> _fields = new(StringComparer.Ordinal);

    public string? DefaultOrderBy => defaultOrderBy;
    public IEnumerable<string> Keys => _fields.Keys;

    public SortFields<T> Add<TKey>(string key, Expression<Func<T, TKey>> selector)
    {
        _fields.Add(key, (query, descending) => descending ? query.OrderByDescending(selector) : query.OrderBy(selector));
        return this;
    }

    /// <summary>
    /// <paramref name="orderBy"/> is <c>key</c> or <c>-key</c>. Ends on the unique <paramref name="tieBreaker"/>
    /// so paging never repeats or drops rows. An unknown key is a validation error, not a silent fallback.
    /// </summary>
    public IOrderedQueryable<T> Apply<TTie>(IQueryable<T> query, string? orderBy, Expression<Func<T, TTie>> tieBreaker)
    {
        var effective = string.IsNullOrEmpty(orderBy) ? defaultOrderBy : orderBy;
        if (string.IsNullOrEmpty(effective))
        {
            return query.OrderBy(tieBreaker);
        }

        var descending = effective[0] == '-';
        var key = descending ? effective[1..] : effective;
        if (!_fields.TryGetValue(key, out var order))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(PagedQuery.OrderBy), $"Can't sort by '{key}'. Sort by one of: {string.Join(", ", _fields.Keys)}.")
            ]);
        }

        return order(query, descending).ThenBy(tieBreaker);
    }
}
