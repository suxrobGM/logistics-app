using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Commands;

internal sealed class UpdateDefaultFeaturesHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<UpdateDefaultFeaturesCommand, Result>
{
    public async Task<Result> Handle(UpdateDefaultFeaturesCommand req, CancellationToken ct)
    {
        // Get all existing default configs
        var existingConfigs = await masterUow.Repository<DefaultFeatureConfig>()
            .Query()
            .ToListAsync(ct);

        foreach (var update in req.Features)
        {
            var config = existingConfigs.FirstOrDefault(c => c.Feature == update.Feature);

            if (config is null)
            {
                config = new DefaultFeatureConfig
                {
                    Feature = update.Feature,
                    IsEnabledByDefault = update.IsEnabledByDefault
                };
                await masterUow.Repository<DefaultFeatureConfig>().AddAsync(config, ct);
            }
            else
            {
                config.IsEnabledByDefault = update.IsEnabledByDefault;
                masterUow.Repository<DefaultFeatureConfig>().Update(config);
            }
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
