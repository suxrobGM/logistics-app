using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTruckQuery : IAppRequest<Result<TruckDto>>
{
    public Guid? TruckOrDriverId { get; set; }
    public bool IncludeLoads { get; set; }
    public bool OnlyActiveLoads { get; set; }
}
