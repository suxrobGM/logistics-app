using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetTruckQuery : IRequest<Result<TruckDto>>
{
    public string? TruckOrDriverId { get; set; }
    public bool IncludeLoads { get; set; }
    public bool OnlyActiveLoads { get; set; }
}
