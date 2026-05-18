using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Queries;

public class GetLoadBoardConfigurationsQuery : IQuery<Result<List<LoadBoardConfigurationDto>>>
{
}
