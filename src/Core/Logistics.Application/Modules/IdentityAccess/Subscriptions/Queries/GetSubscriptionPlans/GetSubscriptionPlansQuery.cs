using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

public sealed class GetSubscriptionPlansQuery : SearchableQuery, IQuery<PagedResult<SubscriptionPlanDto>>
{
}
