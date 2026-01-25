using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteLineItemHandler(ITenantUnitOfWork tenantUow)
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

        // Recalculate invoice total
        var newTotal = invoice.LineItems
            .Where(li => li.Id != req.LineItemId)
            .Sum(li => li.Amount.Amount * li.Quantity);
        invoice.Total = new Money { Amount = newTotal, Currency = invoice.Total.Currency };
        tenantUow.Repository<Invoice>().Update(invoice);

        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
