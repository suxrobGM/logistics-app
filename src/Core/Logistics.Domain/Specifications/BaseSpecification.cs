using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object?>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }
    public int PageSize { get; private set; }
    public int Page { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool Descending { get; private set; }
    
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyPaging(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
        IsPagingEnabled = true;
    }

    protected void ApplyOrderBy(Expression<Func<T, object?>> orderByExpression, bool descending = false)
    {
        OrderBy = orderByExpression;
        Descending = descending;
    }

    protected void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
    }
}
