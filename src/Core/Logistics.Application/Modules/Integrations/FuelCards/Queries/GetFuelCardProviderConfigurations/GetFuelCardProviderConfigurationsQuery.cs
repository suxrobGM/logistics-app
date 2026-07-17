using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

public class GetFuelCardProviderConfigurationsQuery : IQuery<Result<List<FuelCardProviderConfigurationDto>>>
{
}
