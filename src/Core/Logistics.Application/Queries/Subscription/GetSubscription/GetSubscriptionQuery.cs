using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetSubscriptionQuery : IRequest<Result<SubscriptionDto>>
{
    public Guid Id { get; set; }
}
