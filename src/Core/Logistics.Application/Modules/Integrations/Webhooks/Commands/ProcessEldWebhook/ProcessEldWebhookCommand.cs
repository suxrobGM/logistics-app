using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

/// <summary>The endpoint is anonymous, so the tenant's signature secret is what authenticates it.</summary>
public class ProcessEldWebhookCommand : ICommand
{
    public Guid TenantId { get; set; }
    public EldProviderType ProviderType { get; set; }
    public required string RequestBodyJson { get; set; }
    public string? Signature { get; set; }
    public string? Timestamp { get; set; }
}
