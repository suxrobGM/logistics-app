using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteInvoiceHandler : IAppRequestHandler<DeleteInvoiceCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public DeleteInvoiceHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(
        DeleteInvoiceCommand req, CancellationToken ct)
    {
        var invoice = await _tenantUow.Repository<Invoice>().GetByIdAsync(req.Id);

        if (invoice is null)
        {
            return Result.Fail($"Could not find an invoice with ID {req.Id}");
        }

        _tenantUow.Repository<Invoice>().Delete(invoice);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
