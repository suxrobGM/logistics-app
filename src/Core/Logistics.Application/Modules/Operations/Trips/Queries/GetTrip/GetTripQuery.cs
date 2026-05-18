using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

public sealed class GetTripQuery : IQuery<Result<TripDto>>
{
    public Guid TripId { get; set; }
}
