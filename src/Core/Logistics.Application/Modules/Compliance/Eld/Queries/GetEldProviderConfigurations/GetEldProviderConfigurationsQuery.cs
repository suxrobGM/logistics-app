using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetEldProviderConfigurationsQuery : IQuery<Result<List<EldProviderConfigurationDto>>>
{
}
