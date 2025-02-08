using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateSubscriptionCommand : IRequest<Result>
{
    public SubscriptionStatus Status { get; set; }
    public string TenantId { get; set; } = null!;
    public string PlanId { get; set; } = null!;
}
