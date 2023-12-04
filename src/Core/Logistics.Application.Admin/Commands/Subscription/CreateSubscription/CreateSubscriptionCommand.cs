using Logistics.Shared;
using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class CreateSubscriptionCommand : IRequest<ResponseResult>
{
    public SubscriptionStatus Status { get; set; }
    public string TenantId { get; set; } = default!;
    public string PlanId { get; set; } = default!;
}
