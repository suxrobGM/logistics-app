using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CreateSubscriptionCommand : IAppRequest
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
}
