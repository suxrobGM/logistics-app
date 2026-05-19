using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class DeleteLineItemHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
    : IAppRequestHandler<DeleteLineItemCommand, Result>
{
    public async Task<Result> Handle(DeleteLineItemCommand req, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.Fail("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result.Fail("Cannot modify a paid invoice.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result.Fail("Cannot modify a cancelled invoice.");
        }

        var lineItem = invoice.LineItems.FirstOrDefault(li => li.Id == req.LineItemId);
        if (lineItem is null)
        {
            return Result.Fail("Line item not found.");
        }

        tenantUow.Repository<InvoiceLineItem>().Delete(lineItem);
        invoice.LineItems.Remove(lineItem);

        await taxApplier.ApplyAsync(invoice, ct);
        tenantUow.Repository<Invoice>().Update(invoice);

        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
