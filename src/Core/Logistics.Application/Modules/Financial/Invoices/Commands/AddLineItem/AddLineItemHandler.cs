using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class AddLineItemHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
    : IRequestHandler<AddLineItemCommand, Result<InvoiceLineItemDto>>
{
    public async Task<Result<InvoiceLineItemDto>> Handle(AddLineItemCommand req, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<InvoiceLineItemDto>.Fail("Invoice not found.", ErrorCodes.NotFound);
        }

        if (invoice.GetEditBlockedReason() is { } editBlocked)
        {
            return Result<InvoiceLineItemDto>.Fail(editBlocked);
        }

        var lineItem = invoice.AddLineItem(req.Description, req.Type, req.Amount, req.Quantity, req.Notes);
        await tenantUow.Repository<InvoiceLineItem>().AddAsync(lineItem, ct);

        await taxApplier.ApplyAsync(invoice, ct);

        await tenantUow.SaveChangesAsync(ct);

        return Result<InvoiceLineItemDto>.Ok(lineItem.ToDto());
    }
}
