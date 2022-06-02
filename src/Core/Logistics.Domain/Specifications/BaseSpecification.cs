using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : class, IAggregateRoot
{
#pragma warning disable CS8618
    public BaseSpecification(Expression<Func<TEntity, bool>> criteria)
#pragma warning restore CS8618
    {
        Criteria = criteria;
    }

    public Expression<Func<TEntity, bool>> Criteria { get; private set; }

    public List<Expression<Func<TEntity, object>>> Includes { get; } = new List<Expression<Func<TEntity, object>>>();

    public Expression<Func<TEntity, object>> OrderBy { get; private set; }

    public Expression<Func<TEntity, object>> OrderByDescending { get; private set; }

    protected void AddIncludes(Expression<Func<TEntity, object>> expression)
    {
        Includes.Add(expression);
    }

    protected void ApplyOrderBy(Expression<Func<TEntity, object>> orderBy)
    {
        OrderBy = orderBy; 
    }

    protected void ApplyOrderByDescending(Expression<Func<TEntity, object>> orderByDesc)
    {
        OrderByDescending = orderByDesc;
    }
}
