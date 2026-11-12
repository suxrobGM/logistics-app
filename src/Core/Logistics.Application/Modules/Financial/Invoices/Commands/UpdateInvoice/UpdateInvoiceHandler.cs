using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class UpdateInvoiceHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<UpdateInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        UpdateInvoiceCommand req, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.Id, ct);

        if (invoice is null)
        {
            return Result.Fail($"Could not find an invoice with ID '{req.Id}'");
        }

        if (req.InvoiceStatus.HasValue && invoice.Status != req.InvoiceStatus)
        {
            invoice.Status = req.InvoiceStatus.Value;
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
