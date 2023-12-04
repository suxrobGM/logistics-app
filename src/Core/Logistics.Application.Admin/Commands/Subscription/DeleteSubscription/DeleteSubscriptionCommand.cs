using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class DeleteSubscriptionCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
}
