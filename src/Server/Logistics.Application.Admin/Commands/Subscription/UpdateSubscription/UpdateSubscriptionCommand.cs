using Logistics.Shared;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class UpdateSubscriptionCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
    public SubscriptionStatus? Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
