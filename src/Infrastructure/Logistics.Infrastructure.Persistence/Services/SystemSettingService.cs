using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.Services;

internal sealed class SystemSettingService(IMasterUnitOfWork masterUow) : ISystemSettingService
{
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var setting = await masterUow.Repository<SystemSetting>()
            .GetAsync(s => s.Key == key, ct);
        return setting?.Value;
    }

    public async Task SetAsync(string key, string value, string? description = null, CancellationToken ct = default)
    {
        var setting = await masterUow.Repository<SystemSetting>()
            .GetAsync(s => s.Key == key, ct);

        if (setting is null)
        {
            await masterUow.Repository<SystemSetting>().AddAsync(new SystemSetting
            {
                Key = key,
                Value = value,
                Description = description,
                UpdatedAt = DateTime.UtcNow
            }, ct);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;

            if (description is not null)
                setting.Description = description;
        }

        await masterUow.SaveChangesAsync(ct);
    }
}
