using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateSubscriptionCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public SubscriptionStatus? Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
