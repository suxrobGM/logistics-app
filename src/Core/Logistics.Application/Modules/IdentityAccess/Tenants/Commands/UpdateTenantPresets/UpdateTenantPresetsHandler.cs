using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UpdateTenantPresetsHandler(
    IMasterUnitOfWork masterUow,
    IFeatureService featureService)
    : IAppRequestHandler<UpdateTenantPresetsCommand, Result>
{
    public async Task<Result> Handle(UpdateTenantPresetsCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.TenantId}'");
        }

        tenant.Presets = [.. req.Presets.Distinct()];
        await featureService.ApplyPresetFeaturesAsync(tenant.Id, tenant.Presets);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
