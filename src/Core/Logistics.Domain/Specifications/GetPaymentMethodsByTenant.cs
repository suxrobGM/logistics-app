using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetPaymentMethodsByTenant : BaseSpecification<PaymentMethod>
{
    public GetPaymentMethodsByTenant(
        string tenantId,
        string? orderBy)
    {
        Criteria = i => i.TenantId == tenantId;
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
