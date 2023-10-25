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
            "paymentamount" => i => i.Payment.Amount,
            _ => i => i.Created
        };
    }
}
