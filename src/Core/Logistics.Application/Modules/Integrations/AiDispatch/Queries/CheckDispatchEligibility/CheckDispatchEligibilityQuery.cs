using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

public class CheckDispatchEligibilityQuery : IQuery<Result<EligibilityResultDto>>
{
    public Guid TruckId { get; set; }
    public Guid LoadId { get; set; }
    public Guid? DriverId { get; set; }
}
