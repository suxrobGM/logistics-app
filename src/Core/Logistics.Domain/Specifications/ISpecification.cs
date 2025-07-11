using System.Linq.Expressions;
using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Specifications;

public interface ISpecification<T>
{
    // Expression<Func<T, bool>>? Criteria { get; }
    // Expression<Func<T, object>>? GroupBy { get; }
    // Expression<Func<T, object?>>? OrderBy { get; }
    // IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    //
    // bool IsDescending { get; }
    //
    // // Paging is performed only if PageSize > 0
    // int Page { get; }
    // int PageSize { get; }
    
    // business or query filter
    Expression<Func<T, bool>>? Criteria { get; }

    // navigation properties to eager-load
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// Sort expression to order the results.
    /// </summary>
    Sort? Sort { get; }
    
    /// <summary>
    /// Pagination information for the results.
    /// </summary>
    Page? Page { get; }
}
