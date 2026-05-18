using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Tax.Commands;

internal sealed class DeleteTenantTaxRateHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteTenantTaxRateCommand, Result>
{
    public async Task<Result> Handle(DeleteTenantTaxRateCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var rate = await masterUow.Repository<TenantTaxRate>().GetByIdAsync(req.Id, ct);

        if (rate is null || rate.TenantId != tenant.Id)
        {
            return Result.Fail($"Tax rate '{req.Id}' not found.");
        }

        masterUow.Repository<TenantTaxRate>().Delete(rate);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
