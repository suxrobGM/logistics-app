using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class CreateEldProviderConfigurationCommand : ICommand
{
    public EldProviderType ProviderType { get; set; }
    public required string ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
}
