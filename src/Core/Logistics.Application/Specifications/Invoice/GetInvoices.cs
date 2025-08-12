using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

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

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    // protected override Expression<Func<Invoice, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "total" => i => i.Total,
    //         "type" => i => i.Type,
    //         "status" => i => i.Status,
    //         "duedate" => i => i.DueDate,
    //         "notes" => i => i.Notes,
    //         "createdat" => i => i.CreatedAt,
    //         _ => i => i.Status
    //     };
    // }
}
