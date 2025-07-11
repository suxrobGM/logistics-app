using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Specifications;

public sealed class GetInvoices : BaseSpecification<Invoice>
{
    public GetInvoices(
        InvoiceType? filterByType,
        string? orderBy,
        int page,
        int pageSize)
    {
        if (filterByType is not null)
        {
            Criteria = i => i.Type == filterByType;
        }
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Invoice, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "total" => i => i.Total,
            "type" => i => i.Type,
            "status" => i => i.Status,
            "duedate" => i => i.DueDate,
            "notes" => i => i.Notes,
            "createdat" => i => i.CreatedAt,
            _ => i => i.Status
        };
    }
}
