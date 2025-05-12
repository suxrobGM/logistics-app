using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateSubscriptionCommand : IRequest<Result>
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
}
