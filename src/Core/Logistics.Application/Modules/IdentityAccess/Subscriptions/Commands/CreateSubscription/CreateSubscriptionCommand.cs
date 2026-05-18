using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

public class CreateSubscriptionCommand : ICommand
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public int? TrialDays { get; set; }
}
