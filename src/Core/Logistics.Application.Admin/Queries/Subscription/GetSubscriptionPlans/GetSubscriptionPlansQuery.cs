using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetSubscriptionPlansQuery : SearchableQuery, IRequest<PagedResult<SubscriptionPlanDto>>
{
}
