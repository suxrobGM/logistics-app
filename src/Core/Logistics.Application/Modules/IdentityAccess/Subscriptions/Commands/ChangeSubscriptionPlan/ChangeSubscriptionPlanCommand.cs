using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

public class ChangeSubscriptionPlanCommand : ICommand
{
    public Guid SubscriptionId { get; set; }
    public Guid NewPlanId { get; set; }
}
