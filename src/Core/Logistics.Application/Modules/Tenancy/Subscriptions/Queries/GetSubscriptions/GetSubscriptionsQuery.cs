using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

public sealed class GetSubscriptionsQuery : SearchableQuery, IQuery<PagedResult<SubscriptionDto>>
{
}
