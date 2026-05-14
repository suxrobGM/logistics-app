using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetSubscriptionPlanQuery : IQuery<Result<SubscriptionPlanDto>>
{
    public Guid Id { get; set; }
}
