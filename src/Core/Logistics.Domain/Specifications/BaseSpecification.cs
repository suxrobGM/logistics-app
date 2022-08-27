using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : class, IAggregateRoot
{
    public Expression<Func<TEntity, bool>> Criteria { get; protected set; } = root => true;
    public List<Expression<Func<TEntity, object>>> Includes { get; } = new();
    public Expression<Func<TEntity, object>> OrderBy { get; set; } = entity => entity.Id;
}
