using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Tax.Commands;

internal sealed class UpdateTenantTaxRateHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateTenantTaxRateCommand, Result<TenantTaxRateDto>>
{
    public async Task<Result<TenantTaxRateDto>> Handle(UpdateTenantTaxRateCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var rate = await masterUow.Repository<TenantTaxRate>().GetByIdAsync(req.Id, ct);

        if (rate is null || rate.TenantId != tenant.Id)
        {
            return Result<TenantTaxRateDto>.Fail($"Tax rate '{req.Id}' not found.");
        }

        rate.RatePercent = req.RatePercent;
        rate.Description = req.Description;
        if (req.EffectiveFrom.HasValue) rate.EffectiveFrom = req.EffectiveFrom.Value;
        rate.EffectiveTo = req.EffectiveTo;

        masterUow.Repository<TenantTaxRate>().Update(rate);
        await masterUow.SaveChangesAsync(ct);

        return Result<TenantTaxRateDto>.Ok(rate.ToDto());
    }
}
