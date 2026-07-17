using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

[RequiresFeature(TenantFeature.FuelCards)]
public class CreateFuelCardProviderConfigurationCommand : ICommand<Result>
{
    public FuelCardProviderType ProviderType { get; set; }
    public required string ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? ExternalAccountId { get; set; }
}
