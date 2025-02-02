using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    Expression<Func<T, object?>>? OrderBy { get; }
    Expression<Func<T, object>>? GroupBy { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    int PageSize { get; }
    int Page { get; }
    bool IsPagingEnabled { get; }
    bool IsDescending { get; }
}
