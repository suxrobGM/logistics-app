using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetSubscriptionsQuery : SearchableQuery, IRequest<PagedResult<SubscriptionDto>>
{
}
