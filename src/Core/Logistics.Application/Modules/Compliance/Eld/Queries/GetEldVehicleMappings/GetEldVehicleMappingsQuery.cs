using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetEldVehicleMappingsQuery : IQuery<Result<List<EldVehicleMappingDto>>>
{
    public Guid ProviderId { get; set; }
}
