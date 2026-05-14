using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CreateSubscriptionCommand : ICommand
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public int? TrialDays { get; set; }
}
