using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

public sealed class GetSubscriptionPlansQuery : SearchableQuery, IQuery<PagedResult<SubscriptionPlanDto>>
{
}
