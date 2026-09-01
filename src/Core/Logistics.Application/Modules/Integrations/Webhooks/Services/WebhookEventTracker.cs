using System.Security.Cryptography;
using System.Text;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Webhooks.Services;

internal sealed class WebhookEventTracker(IMasterUnitOfWork masterUow) : IWebhookEventTracker
{
    public async Task<bool> WasAlreadyHandledAsync(
        string provider, string eventKey, CancellationToken ct = default)
    {
        return await masterUow.Repository<ProcessedWebhookEvent>()
            .GetAsync(e => e.Provider == provider && e.EventKey == eventKey, ct) is not null;
    }

    public async Task MarkHandledAsync(string provider, string eventKey, CancellationToken ct = default)
    {
        await masterUow.Repository<ProcessedWebhookEvent>()
            .AddAsync(new ProcessedWebhookEvent { Provider = provider, EventKey = eventKey }, ct);
        await masterUow.SaveChangesAsync(ct);
    }

    public string BuildKeyFromBody(string requestBody)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(requestBody)));
    }
}
