using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public sealed class FilterInvoicesByInterval : BaseSpecification<Invoice>
{
    public FilterInvoicesByInterval(
        string? orderProperty,
        DateTime? startPeriod,
        DateTime endPeriod,
        int page,
        int pageSize,
        bool descending)
    {
        Criteria = i =>
            i.CreateDate >= startPeriod && i.CreateDate <= endPeriod;
        
        ApplyOrderBy(InitOrderBy(orderProperty), descending);
    }
    
    private static Expression<Func<Invoice, object?>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower();
        return propertyName switch
        {
            "loadref" => i => i.Load.RefId,
            "paymentamount" => i => i.Payment.Amount,
            "paymentdate" => i => i.Payment.PaymentDate,
            "customername" => i => i.Customer.Name,
            "createddate" => i => i.CreateDate,
            _ => i => i.Payment.Status
        };
    }
}
