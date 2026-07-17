using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Webhooks.Services;

internal sealed class ProcessedWebhookEventCleanupService(
    IMasterUnitOfWork masterUow,
    ILogger<ProcessedWebhookEventCleanupService> logger) : IProcessedWebhookEventCleanupService
{
    private const int RetentionDays = 30;

    public async Task CleanupAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);

        var stale = await masterUow.Repository<ProcessedWebhookEvent>()
            .Query()
            .Where(e => e.ReceivedAt < cutoff)
            .ToListAsync(ct);

        if (stale.Count == 0)
        {
            return;
        }

        foreach (var evt in stale)
        {
            masterUow.Repository<ProcessedWebhookEvent>().Delete(evt);
        }

        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Pruned {Count} processed webhook events older than {RetentionDays} days",
            stale.Count, RetentionDays);
    }
}
