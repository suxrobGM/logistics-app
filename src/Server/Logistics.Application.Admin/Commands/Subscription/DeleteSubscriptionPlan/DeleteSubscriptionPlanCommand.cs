using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class DeleteSubscriptionPlanCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
}
