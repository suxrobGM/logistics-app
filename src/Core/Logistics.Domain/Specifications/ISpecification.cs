using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object?>> OrderBy { get; }
    bool Descending { get; }
}
