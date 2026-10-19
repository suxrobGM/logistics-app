using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class UpdateLineItemHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
    : IRequestHandler<UpdateLineItemCommand, Result<InvoiceLineItemDto>>
{
    public async Task<Result<InvoiceLineItemDto>> Handle(UpdateLineItemCommand req, CancellationToken ct)
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

        var lineItem = invoice.LineItems.FirstOrDefault(li => li.Id == req.LineItemId);
        if (lineItem is null)
        {
            return Result<InvoiceLineItemDto>.Fail("Line item not found.");
        }

        // Update only provided fields
        if (req.Description is not null)
        {
            lineItem.Description = req.Description;
        }

        if (req.Type.HasValue)
        {
            lineItem.Type = req.Type.Value;
        }

        if (req.Amount.HasValue)
        {
            lineItem.Amount = lineItem.Amount with { Amount = req.Amount.Value };
        }

        if (invoice is PayrollInvoice)
        {
            lineItem.Amount = lineItem.Amount with
            {
                Amount = PayrollInvoice.SignedLineAmount(lineItem.Type, lineItem.Amount.Amount)
            };
        }

        if (req.Quantity.HasValue)
        {
            lineItem.Quantity = req.Quantity.Value;
        }

        if (req.Notes is not null)
        {
            lineItem.Notes = req.Notes;
        }

        tenantUow.Repository<InvoiceLineItem>().Update(lineItem);

        await taxApplier.ApplyAsync(invoice, ct);

        tenantUow.Repository<Invoice>().Update(invoice);

        await tenantUow.SaveChangesAsync(ct);

        return Result<InvoiceLineItemDto>.Ok(lineItem.ToDto());
    }
}
