using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetSubscriptionQuery : IAppRequest<Result<SubscriptionDto>>
{
    public Guid Id { get; set; }
}
