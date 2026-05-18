using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

public class GetTruckQuery : IQuery<Result<TruckDto>>
{
    public Guid? TruckOrDriverId { get; set; }
    public bool IncludeLoads { get; set; }
    public bool OnlyActiveLoads { get; set; }
}
