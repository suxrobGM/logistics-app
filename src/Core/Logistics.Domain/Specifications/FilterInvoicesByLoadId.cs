using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public sealed class FilterInvoicesByLoadId : BaseSpecification<LoadInvoice>
{
    public FilterInvoicesByLoadId(
        Guid loadId,
        string? orderBy,
        int page,
        int pageSize)
    {
        Criteria = i => i.LoadId == loadId;
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<LoadInvoice, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "load.number" => i => i.Load.Number,
            "total" => i => i.Total,
            "status" => i => i.Status,
            "customer.name" => i => i.Customer.Name,
            "createdat" => i => i.CreatedAt,
            _ => i => i.Status
        };
    }
}
