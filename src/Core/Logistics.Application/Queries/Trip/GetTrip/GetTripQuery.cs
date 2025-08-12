using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetTripQuery : IRequest<Result<TripDto>>
{
    public Guid TripId { get; set; }
}
