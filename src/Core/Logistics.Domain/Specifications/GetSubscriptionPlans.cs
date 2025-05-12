using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetSubscriptionPlans : BaseSpecification<SubscriptionPlan>
{
    public GetSubscriptionPlans(
        string? orderBy,
        int page,
        int pageSize)
    {
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<SubscriptionPlan, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "name" => i => i.Name,
            "description" => i => i.Description,
            "price" => i => i.Price,
            _ => i => i.CreatedAt
        };
    }
}
