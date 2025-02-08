using Logistics.Shared.Models;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetSubscriptionQuery : IRequest<Result<SubscriptionDto>>
{
    public string Id { get; set; } = null!;
}
