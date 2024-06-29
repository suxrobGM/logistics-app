using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public sealed class FilterInvoicesByInterval : BaseSpecification<Invoice>
{
    public FilterInvoicesByInterval(
        string? orderBy,
        DateTime? startPeriod,
        DateTime endPeriod,
        int page,
        int pageSize)
    {
        Criteria = i =>
            i.CreateDate >= startPeriod && i.CreateDate <= endPeriod;
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Invoice, object?>> CreateOrderByExpression(string propertyName)
    {
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
