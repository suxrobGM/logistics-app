using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

public sealed class GetSubscriptionQuery : IQuery<Result<SubscriptionDto>>
{
    public Guid Id { get; set; }
}
