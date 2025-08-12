using System.Linq.Expressions;

using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T> where T : IEntity<Guid>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];

    protected BaseSpecification() { }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
        => Criteria = criteria;

    public Expression<Func<T, bool>>? Criteria { get; protected init; }
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    public Sort? Sort { get; private set; }
    public Page? Page { get; private set; } = new();


    #region Fluent helpers 

    protected void AddInclude(Expression<Func<T, object>> navigation)
        => _includes.Add(navigation);

    /// <summary>
    /// Sets the order by expression for the results.
    /// If the orderBy string starts with a '-', it will be treated as descending order.
    /// Otherwise, it will be treated as an ascending order.
    /// If the orderBy string is null or empty, no ordering will be applied.
    /// </summary>
    /// <param name="orderBy">Property name to order by. Prefix with '-' for descending order.</param>
    protected void OrderBy(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return;
        }

        var desc = orderBy[0] == '-';
        var prop = desc ? orderBy[1..] : orderBy;
        Sort = new Sort(prop, desc);
    }

    protected void ApplyPaging(int number, int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(number, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        Page = new Page(number, size);
    }

    #endregion
}
