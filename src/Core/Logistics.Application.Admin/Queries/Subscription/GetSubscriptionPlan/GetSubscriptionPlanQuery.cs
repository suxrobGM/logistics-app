using Logistics.Shared.Models;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetSubscriptionPlanQuery : IRequest<Result<SubscriptionPlanDto>>
{
    public string Id { get; set; } = default!;
}
