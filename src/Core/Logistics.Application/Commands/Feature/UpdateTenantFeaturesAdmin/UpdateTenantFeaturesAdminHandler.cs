using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTenantFeaturesAdminHandler(
    IMasterUnitOfWork masterUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<UpdateTenantFeaturesAdminCommand, Result>
{
    public async Task<Result> Handle(UpdateTenantFeaturesAdminCommand req, CancellationToken ct)
    {
        // Verify tenant exists
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);
        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.TenantId}'");
        }

        // Get existing configs for this tenant
        var existingConfigs = await masterUow.Repository<TenantFeatureConfig>()
            .GetListAsync(c => c.TenantId == req.TenantId, ct);

        var userId = currentUserService.GetUserId().ToString();
        var now = DateTime.UtcNow;

        foreach (var update in req.Features)
        {
            var config = existingConfigs.FirstOrDefault(c => c.Feature == update.Feature);

            if (config is null)
            {
                config = new TenantFeatureConfig
                {
                    TenantId = req.TenantId,
                    Feature = update.Feature,
                    IsEnabled = update.IsEnabled,
                    IsAdminLocked = update.IsAdminLocked,
                    UpdatedBy = userId,
                    UpdatedAt = now
                };
                await masterUow.Repository<TenantFeatureConfig>().AddAsync(config, ct);
            }
            else
            {
                config.IsEnabled = update.IsEnabled;
                config.IsAdminLocked = update.IsAdminLocked;
                config.UpdatedBy = userId;
                config.UpdatedAt = now;
                masterUow.Repository<TenantFeatureConfig>().Update(config);
            }
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
