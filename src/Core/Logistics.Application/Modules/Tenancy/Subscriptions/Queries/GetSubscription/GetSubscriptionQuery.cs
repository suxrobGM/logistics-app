using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

public sealed class GetSubscriptionQuery : IQuery<Result<SubscriptionDto>>
{
    public Guid Id { get; set; }
}
