using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetLoadBoardConfigurationsQuery : IAppRequest<Result<List<LoadBoardConfigurationDto>>>
{
}
