using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetTruckQuery : IRequest<ResponseResult<TruckDto>>
{
    public string? TruckOrDriverId { get; set; }
    public bool IncludeLoads { get; set; }
    public bool OnlyActiveLoads { get; set; }
}
