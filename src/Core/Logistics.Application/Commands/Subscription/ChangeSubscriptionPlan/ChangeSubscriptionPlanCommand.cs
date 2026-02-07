using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class ChangeSubscriptionPlanCommand : IAppRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid NewPlanId { get; set; }
}
