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

    /// <summary>The account id in the provider's system. Required for TT ELD (the carrier's USDOT number).</summary>
    public string? ExternalAccountId { get; set; }

    /// <summary>Samsara data region: <c>us</c> (default), <c>eu</c> or <c>ca</c>. Ignored by other providers.</summary>
    public string? Region { get; set; }
}
