using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetSubscriptionPaymentsById : BaseSpecification<SubscriptionPayment>
{
    public GetSubscriptionPaymentsById(
        string subscriptionId,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (string.IsNullOrEmpty(orderBy))
        {
            orderBy = "paymentdate";
        }
        
        Criteria = i => i.SubscriptionId == subscriptionId;
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<SubscriptionPayment, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "paymentdate" => i => i.PaymentDate,
            "method" => i => i.Method,
            "amount" => i => i.Amount,
            "status" => i => i.Status,
            "billingaddress" => i => i.BillingAddress,
            _ => i => i.CreatedDate
        };
    }
}
