using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : class, IAggregateRoot
{
    protected BaseSpecification()
    {
        Criteria = entity => true;
        OrderBy = entity => entity.Id;
        Includes = new();
    }

    public Expression<Func<TEntity, bool>> Criteria { get; protected init; }
    public List<Expression<Func<TEntity, object>>> Includes { get; }
    public Expression<Func<TEntity, object>> OrderBy { get; protected init; }
}
