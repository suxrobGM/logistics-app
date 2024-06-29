using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class UpdateSubscriptionPlanCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
}
