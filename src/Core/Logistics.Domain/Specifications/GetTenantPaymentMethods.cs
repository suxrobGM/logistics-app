using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetTenantPaymentMethods : BaseSpecification<PaymentMethod>
{
    public GetTenantPaymentMethods(string? orderBy)
    {
        ApplyOrderBy(orderBy);
    }
    
    protected override Expression<Func<PaymentMethod, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "type" => i => i.Type,
            _ => i => i.CreatedDate
        };
    }
}
