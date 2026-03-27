using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class CreateEldProviderConfigurationCommand : IAppRequest
{
    public EldProviderType ProviderType { get; set; }
    public required string ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
}
