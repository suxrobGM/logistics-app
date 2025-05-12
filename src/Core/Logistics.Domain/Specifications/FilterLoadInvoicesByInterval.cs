using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public sealed class FilterLoadInvoicesByInterval : BaseSpecification<LoadInvoice>
{
    public FilterLoadInvoicesByInterval(
        string? orderBy,
        DateTime? startPeriod,
        DateTime endPeriod,
        int page,
        int pageSize)
    {
        Criteria = i =>
            i.CreatedAt >= startPeriod && i.CreatedAt <= endPeriod;
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<LoadInvoice, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "loadref" => i => i.Load.Number,
            "total" => i => i.Total,
            "status" => i => i.Status,
            "customername" => i => i.Customer.Name,
            "createdat" => i => i.CreatedAt,
            _ => i => i.Status
        };
    }
}
