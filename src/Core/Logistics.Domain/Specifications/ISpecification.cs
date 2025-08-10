using System.Linq.Expressions;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Specifications;

public interface ISpecification<T>
{
    /// <summary>
    /// Criteria or predicate to filter the entities.
    /// If null, no filtering will be applied.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Related entities to include in the query results.
    /// </summary>
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
