using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetEldProviderVehiclesQuery : IQuery<Result<List<EldVehicleDto>>>
{
    public Guid ProviderId { get; set; }
}
