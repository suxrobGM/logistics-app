
namespace Logistics.Application.Modules.Tenancy.Subscriptions.Commands;

public class ChangeSubscriptionPlanCommand : ICommand
{
    public Guid SubscriptionId { get; set; }
    public Guid NewPlanId { get; set; }
}
