using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

[RequiresFeature(TenantFeature.FuelCards)]
public class GetFuelCardProviderConfigurationsQuery : IQuery<Result<List<FuelCardProviderConfigurationDto>>>
{
}
