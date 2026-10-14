using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

/// <summary>
/// An inbound ELD webhook. The endpoint is anonymous, so the tenant comes from the per-tenant URL
/// the tenant registered with the provider (<c>/webhooks/eld/{provider}/{tenantId}</c>) and the
/// signature, checked against that tenant's secret, is what authenticates it.
/// </summary>
public class ProcessEldWebhookCommand : ICommand
{
    public Guid TenantId { get; set; }
    public EldProviderType ProviderType { get; set; }
    public required string RequestBodyJson { get; set; }
    public string? Signature { get; set; }
    public string? Timestamp { get; set; }
}
