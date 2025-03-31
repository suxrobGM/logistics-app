using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateSubscriptionCommand : IRequest<Result>
{
    public string TenantId { get; set; } = null!;
    public string PlanId { get; set; } = null!;
}
