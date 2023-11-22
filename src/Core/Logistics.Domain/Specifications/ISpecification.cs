using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object?>>? OrderBy { get; }
    public Expression<Func<T, object>>? GroupBy { get; }
    int PageSize { get; }
    int Page { get; }
    bool IsPagingEnabled { get; }
    bool Descending { get; }
}
