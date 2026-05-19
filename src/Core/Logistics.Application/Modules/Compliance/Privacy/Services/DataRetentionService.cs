using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Privacy.Services;

internal sealed class DataRetentionService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ILogger<DataRetentionService> logger) : IDataRetentionService
{
    public async Task PurgeAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var notificationCutoff = now - PrivacyDefaults.NotificationRetention;
        var dispatchCutoff = now - PrivacyDefaults.AiDispatchSessionRetention;

        var tenants = await masterUow.Repository<Tenant>().GetListAsync(ct: ct);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                tenantUow.SetCurrentTenant(tenant);

                var oldNotifications = await tenantUow.Repository<Notification>()
                    .Query()
                    .Where(n => n.IsRead && n.CreatedDate < notificationCutoff)
                    .ToListAsync(ct);

                foreach (var notification in oldNotifications)
                {
                    tenantUow.Repository<Notification>().Delete(notification);
                }

                var oldSessions = await tenantUow.Repository<AiDispatchSession>()
                    .Query()
                    .Where(s => s.StartedAt < dispatchCutoff)
                    .ToListAsync(ct);

                foreach (var session in oldSessions)
                {
                    tenantUow.Repository<AiDispatchSession>().Delete(session);
                }

                if (oldNotifications.Count > 0 || oldSessions.Count > 0)
                {
                    await tenantUow.SaveChangesAsync(ct);
                    logger.LogInformation(
                        "Retention purge for tenant '{TenantName}': {Notifications} notifications, {Sessions} dispatch sessions.",
                        tenant.Name, oldNotifications.Count, oldSessions.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Retention purge failed for tenant '{TenantName}'.", tenant.Name);
            }
        }
    }
}
