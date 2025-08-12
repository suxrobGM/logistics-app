using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetSubscriptionPlansQuery : SearchableQuery, IAppRequest<PagedResult<SubscriptionPlanDto>>
{
}
