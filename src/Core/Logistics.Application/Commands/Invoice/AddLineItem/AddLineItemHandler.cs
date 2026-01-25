using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class AddLineItemHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<AddLineItemCommand, Result<InvoiceLineItemDto>>
{
    public async Task<Result<InvoiceLineItemDto>> Handle(AddLineItemCommand req, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<InvoiceLineItemDto>.Fail("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<InvoiceLineItemDto>.Fail("Cannot modify a paid invoice.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result<InvoiceLineItemDto>.Fail("Cannot modify a cancelled invoice.");
        }

        var nextOrder = invoice.LineItems.Any() ? invoice.LineItems.Max(li => li.Order) + 1 : 0;

        var lineItem = new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = req.Description,
            Type = req.Type,
            Amount = invoice.Total with { Amount = req.Amount },
            Quantity = req.Quantity,
            Order = nextOrder,
            Notes = req.Notes
        };

        await tenantUow.Repository<InvoiceLineItem>().AddAsync(lineItem, ct);

        // Recalculate invoice total
        var newTotal = invoice.LineItems.Sum(li => li.Amount.Amount * li.Quantity) + (req.Amount * req.Quantity);
        invoice.Total = invoice.Total with { Amount = newTotal };
        tenantUow.Repository<Invoice>().Update(invoice);

        await tenantUow.SaveChangesAsync(ct);

        return Result<InvoiceLineItemDto>.Ok(lineItem.ToDto());
    }
}
