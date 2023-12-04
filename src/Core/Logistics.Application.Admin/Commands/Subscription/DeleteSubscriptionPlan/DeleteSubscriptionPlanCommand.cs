using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class DeleteSubscriptionPlanCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
}
