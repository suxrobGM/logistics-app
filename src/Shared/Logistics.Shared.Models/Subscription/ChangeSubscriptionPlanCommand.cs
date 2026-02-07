namespace Logistics.Shared.Models;

public class ChangeSubscriptionPlanCommand
{
    public Guid SubscriptionId { get; set; }
    public Guid NewPlanId { get; set; }
}
