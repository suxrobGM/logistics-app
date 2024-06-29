using System.Linq.Expressions;
using Logistics.Domain.Core;

namespace Logistics.Domain.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T> where T : IEntity<string>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public Expression<Func<T, object?>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public int PageSize { get; private set; }
    public int Page { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool IsDescending { get; private set; }
    
    protected virtual Expression<Func<T, object?>> CreateOrderByExpression(string propertyName)
    {
        return i => i.Id;
    }

    protected void ApplyPaging(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
        IsPagingEnabled = true;
    }

    protected void ApplyOrderBy(string? orderByProperty)
    {
        var parsedOrderByProperty = ParseOrderByProperty(orderByProperty);
        OrderBy = CreateOrderByExpression(parsedOrderByProperty);
    }

    private string ParseOrderByProperty(string? orderByProperty)
    {
        if (string.IsNullOrEmpty(orderByProperty))
        {
            return string.Empty;
        }

        if (orderByProperty.StartsWith('-'))
        {
            IsDescending = true;
            orderByProperty = orderByProperty[1..];
        }
        return orderByProperty.ToLower();
    }
}
