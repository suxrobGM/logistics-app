using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

/// <summary>Dwell and detention at every stop of the load the truck has arrived at, in route order.</summary>
public class GetLoadDetentionQuery : IQuery<Result<List<StopDetentionDto>>>
{
    public Guid LoadId { get; set; }
}
