using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

public sealed class GetTripTimelineQuery : IQuery<Result<TripTimelineDto>>
{
    public Guid TripId { get; set; }
}
