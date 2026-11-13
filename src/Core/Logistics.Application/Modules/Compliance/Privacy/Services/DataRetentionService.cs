using Logistics.Application.Modules.AI.AIDispatch;
using Logistics.Application.Modules.Compliance.Privacy.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Privacy.Services;

internal sealed class DataRetentionService(
    ITenantUnitOfWork tenantUow,
    ILogger<DataRetentionService> logger) : IDataRetentionService
{
    public async Task PurgeAsync(Tenant tenant, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var notificationCutoff = now - PrivacyDefaults.NotificationRetention;
        var dispatchCutoff = now - PrivacyDefaults.AIDispatchSessionRetention;

        var oldNotifications = await tenantUow.Repository<Notification>()
            .Query()
            .Where(n => n.CreatedDate < notificationCutoff)
            .ToListAsync(ct);

        foreach (var notification in oldNotifications)
        {
            tenantUow.Repository<Notification>().Delete(notification);
        }

        var oldSessions = await tenantUow.Repository<AgentSession>()
            .Query()
            .DispatchOnly()
            .Where(s => s.CreatedAt < dispatchCutoff)
            .ToListAsync(ct);

        foreach (var session in oldSessions)
        {
            tenantUow.Repository<AgentSession>().Delete(session);
        }

        if (oldNotifications.Count > 0 || oldSessions.Count > 0)
        {
            await tenantUow.SaveChangesAsync(ct);
            logger.LogInformation(
                "Retention purge for tenant '{TenantName}': {Notifications} notifications, {Sessions} dispatch sessions.",
                tenant.Name, oldNotifications.Count, oldSessions.Count);
        }
    }
}
