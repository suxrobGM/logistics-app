using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateInvoiceHandler : RequestHandler<UpdateInvoiceCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public UpdateInvoiceHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public override async Task<Result> Handle(
        UpdateInvoiceCommand req, CancellationToken ct)
    {
        var invoice = await _tenantUow.Repository<Invoice>().GetByIdAsync(req.Id);

        if (invoice is null)
        {
            return Result.Fail($"Could not find an invoice with ID '{req.Id}'");
        }

        if (req.InvoiceStatus.HasValue && invoice.Status != req.InvoiceStatus)
        {
            invoice.Status = req.InvoiceStatus.Value;
        }

        _tenantUow.Repository<Invoice>().Update(invoice);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
