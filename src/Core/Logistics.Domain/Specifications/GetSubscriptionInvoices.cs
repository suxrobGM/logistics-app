using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class GetSubscriptionInvoices : BaseSpecification<SubscriptionInvoice>
{
    public GetSubscriptionInvoices(
        Guid subscriptionId,
        string? orderBy,
        int page,
        int pageSize)
    {
        Criteria = i => i.SubscriptionId == subscriptionId;
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<SubscriptionInvoice, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "billingperiodstart" => i => i.BillingPeriodStart,
            "billingperiodend" => i => i.BillingPeriodEnd,
            "duedate" => i => i.DueDate,
            "total" => i => i.Total.Amount,
            "invoicenumber" => i => i.Number,
            "status" => i => i.Status,
            "notes" => i => i.Notes,
            "lastmodifiedat" => i => i.LastModifiedAt,
            _ => i => i.CreatedAt
        };
    }
}
