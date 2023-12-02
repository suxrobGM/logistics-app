using Logistics.Shared.Models;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetSubscriptionQuery : IRequest<ResponseResult<SubscriptionDto>>
{
    public string Id { get; set; } = default!;
}
