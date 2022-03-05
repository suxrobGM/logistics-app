using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public class BaseSpecification<T> : ISpecification<T>
{
#pragma warning disable CS8618
    public BaseSpecification()
#pragma warning restore CS8618
    {
    }

#pragma warning disable CS8618
    public BaseSpecification(Expression<Func<T, bool>> criteria)
#pragma warning restore CS8618
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>> Criteria { get; private set; }

    public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

    public Expression<Func<T, object>> OrderBy { get; private set; }

    public Expression<Func<T, object>> OrderByDescending { get; private set; }

    protected void AddIncludes(Expression<Func<T, object>> expression)
    {
        Includes.Add(expression);
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy)
    {
        OrderBy = orderBy;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDesc)
    {
        OrderByDescending = orderByDesc;
    }
}
