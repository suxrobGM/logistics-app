using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetTripTimelineQuery : IAppRequest<Result<TripTimelineDto>>
{
    public Guid TripId { get; set; }
}
