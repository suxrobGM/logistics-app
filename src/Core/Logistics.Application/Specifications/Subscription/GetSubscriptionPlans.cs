using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class GetSubscriptionPlans : BaseSpecification<SubscriptionPlan>
{
    public GetSubscriptionPlans(
        string? orderBy,
        int page,
        int pageSize)
    {
        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    // protected override Expression<Func<SubscriptionPlan, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "name" => i => i.Name,
    //         "description" => i => i.Description,
    //         "price" => i => i.Price,
    //         _ => i => i.CreatedAt
    //     };
    // }
}
