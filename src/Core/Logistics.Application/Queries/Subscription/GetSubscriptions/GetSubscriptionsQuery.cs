using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetSubscriptionsQuery : SearchableQuery, IRequest<PagedResult<SubscriptionDto>>
{
}
