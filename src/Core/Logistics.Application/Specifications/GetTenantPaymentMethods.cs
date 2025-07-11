using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class GetTenantPaymentMethods : BaseSpecification<PaymentMethod>
{
    public GetTenantPaymentMethods(string? orderBy)
    {
        OrderBy(orderBy);
    }
    
    // protected override Expression<Func<PaymentMethod, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "type" => i => i.Type,
    //         _ => i => i.CreatedDate
    //     };
    // }
}
