using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEldProviderConfigurationsQuery : IAppRequest<Result<List<EldProviderConfigurationDto>>>
{
}
