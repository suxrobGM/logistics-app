using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Queries;

public class CheckDispatchEligibilityQuery : IQuery<Result<EligibilityResultDto>>
{
    public Guid TruckId { get; set; }
    public Guid LoadId { get; set; }
    public Guid? DriverId { get; set; }
}
