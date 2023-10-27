using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class FilterInvoicesByInterval : BaseSpecification<Invoice>
{
    public FilterInvoicesByInterval(string? orderProperty, DateTime? startPeriod, DateTime endPeriod, bool descending)
    {
        Descending = descending;
        OrderBy = InitOrderBy(orderProperty);

        Criteria = i =>
            i.Created >= startPeriod && i.Created <= endPeriod;
    }
    
    private static Expression<Func<Invoice, object>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower();
        return propertyName switch
        {
            "loadref" => i => i.Load.RefId,
            "paymentamount" => i => i.Payment.Amount,
            "paymentdate" => i => i.Payment.PaymentDate!,
            "customername" => i => i.Customer.Name,
            _ => i => i.Created
        };
    }
}
