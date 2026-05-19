using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class AddLineItemHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
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
        invoice.LineItems.Add(lineItem);

        await taxApplier.ApplyAsync(invoice, ct);
        tenantUow.Repository<Invoice>().Update(invoice);

        await tenantUow.SaveChangesAsync(ct);

        return Result<InvoiceLineItemDto>.Ok(lineItem.ToDto());
    }
}
