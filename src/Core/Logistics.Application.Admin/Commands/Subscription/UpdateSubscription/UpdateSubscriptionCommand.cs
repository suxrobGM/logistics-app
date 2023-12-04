using Logistics.Domain.Entities;
using Logistics.Shared;
using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class UpdateSubscriptionCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public SubscriptionStatus? Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
