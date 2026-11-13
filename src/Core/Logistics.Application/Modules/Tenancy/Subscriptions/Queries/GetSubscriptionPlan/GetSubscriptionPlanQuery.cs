using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

public sealed class GetSubscriptionPlanQuery : IQuery<Result<SubscriptionPlanDto>>
{
    public Guid Id { get; set; }
}
