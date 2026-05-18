using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

public sealed class GetSubscriptionPlanQuery : IQuery<Result<SubscriptionPlanDto>>
{
    public Guid Id { get; set; }
}
