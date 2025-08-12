using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetTripQuery : IAppRequest<Result<TripDto>>
{
    public Guid TripId { get; set; }
}
