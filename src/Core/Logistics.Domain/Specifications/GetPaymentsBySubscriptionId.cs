using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetPaymentsBySubscriptionId : BaseSpecification<Payment>
{
    public GetPaymentsBySubscriptionId(
        string subscriptionId,
        string? orderBy,
        int page,
        int pageSize)
    {
        Criteria = i => i.SubscriptionId == subscriptionId;
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Payment, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "paymentdate" => i => i.PaymentDate,
            "method" => i => i.Method,
            "amount" => i => i.Amount,
            "status" => i => i.Status,
            "notes" => i => i.Notes!,
            "paymentfor" => i => i.PaymentFor,
            "billingaddress" => i => i.BillingAddress,
            _ => i => i.CreatedDate
        };
    }
}
