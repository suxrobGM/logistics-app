using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetTripsQuery : PagedQuery, IRequest<PagedResult<TripDto>>
{
    public string? Name { get; set; }
    public TripStatus? Status { get; set; }
    public string? TruckNumber { get; set; }
}
