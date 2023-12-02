using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetSubscriptionPlans : BaseSpecification<SubscriptionPlan>
{
    public GetSubscriptionPlans(
        string? orderProperty,
        int page,
        int pageSize,
        bool descending)
    {
        ApplyOrderBy(InitOrderBy(orderProperty), descending);
        ApplyPaging(page, pageSize);
    }
    
    private static Expression<Func<SubscriptionPlan, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower();
        return propertyName switch
        {
            "name" => i => i.Name,
            "description" => i => i.Description,
            "price" => i => i.Price,
            _ => i => i.CreatedDate
        };
    }
}
