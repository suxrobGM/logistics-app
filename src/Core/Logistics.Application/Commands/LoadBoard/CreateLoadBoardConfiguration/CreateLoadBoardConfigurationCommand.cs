using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class CreateLoadBoardConfigurationCommand : IAppRequest
{
    public LoadBoardProviderType ProviderType { get; set; }
    public required string ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
    public string? CompanyDotNumber { get; set; }
    public string? CompanyMcNumber { get; set; }
}
