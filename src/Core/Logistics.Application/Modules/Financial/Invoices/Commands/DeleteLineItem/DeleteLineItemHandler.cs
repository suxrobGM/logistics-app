using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class DeleteLineItemHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
    : IRequestHandler<DeleteLineItemCommand, Result>
{
    public async Task<Result> Handle(DeleteLineItemCommand req, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.Fail("Invoice not found.", ErrorCodes.NotFound);
        }

        if (invoice.GetEditBlockedReason() is { } editBlocked)
        {
            return Result.Fail(editBlocked);
        }

        var lineItem = invoice.LineItems.FirstOrDefault(li => li.Id == req.LineItemId);
        if (lineItem is null)
        {
            return Result.Fail("Line item not found.", ErrorCodes.NotFound);
        }

        tenantUow.Repository<InvoiceLineItem>().Delete(lineItem);
        invoice.LineItems.Remove(lineItem);

        await taxApplier.ApplyAsync(invoice, ct);

        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
