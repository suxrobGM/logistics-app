using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class FilterPaymentsByInterval : BaseSpecification<Payment>
{
    public FilterPaymentsByInterval(string? orderProperty, DateTime? startPeriod, DateTime endPeriod, bool descending)
    {
        Descending = descending;
        OrderBy = InitOrderBy(orderProperty);

        Criteria = i =>
            i.CreatedDate >= startPeriod && i.CreatedDate <= endPeriod;
    }
    
    private static Expression<Func<Payment, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower();
        return propertyName switch
        {
            "paymentdate" => i => i.PaymentDate,
            "method" => i => i.Method,
            "amount" => i => i.Amount,
            "status" => i => i.Status,
            "comment" => i => i.Comment!,
            "paymentfor" => i => i.PaymentFor,
            "billingaddress" => i => i.BillingAddress,
            _ => i => i.CreatedDate
        };
    }
}
