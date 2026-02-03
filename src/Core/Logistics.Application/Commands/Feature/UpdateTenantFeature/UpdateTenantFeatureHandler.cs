using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTenantFeatureHandler(
    IMasterUnitOfWork masterUow,
    ITenantService tenantService,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<UpdateTenantFeatureCommand, Result>
{
    public async Task<Result> Handle(UpdateTenantFeatureCommand req, CancellationToken ct)
    {
        var tenant = tenantService.GetCurrentTenant();

        // Get existing config or create new one
        var config = await masterUow.Repository<TenantFeatureConfig>()
            .GetAsync(c => c.TenantId == tenant.Id && c.Feature == req.Feature, ct);

        if (config is null)
        {
            config = new TenantFeatureConfig
            {
                TenantId = tenant.Id,
                Feature = req.Feature,
                IsEnabled = req.IsEnabled,
                IsAdminLocked = false,
                UpdatedBy = currentUserService.GetUserId().ToString(),
                UpdatedAt = DateTime.UtcNow
            };
            await masterUow.Repository<TenantFeatureConfig>().AddAsync(config, ct);
        }
        else
        {
            // Check if admin-locked
            if (config.IsAdminLocked)
            {
                var featureName = req.Feature.GetDescription();
                return Result.Fail($"The '{featureName}' feature is locked by an administrator and cannot be changed.");
            }

            config.IsEnabled = req.IsEnabled;
            config.UpdatedBy = currentUserService.GetUserId().ToString();
            config.UpdatedAt = DateTime.UtcNow;
            masterUow.Repository<TenantFeatureConfig>().Update(config);
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
