using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Idempotency ledger for inbound third-party webhooks (master database). A row records that an
/// event, identified by (<see cref="Provider"/>, <see cref="EventKey"/>), was already processed so
/// provider retries are ignored. Rows are pruned by a daily housekeeping job.
/// </summary>
public class ProcessedWebhookEvent : Entity, IMasterEntity
{
    public required string Provider { get; set; }
    public required string EventKey { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
