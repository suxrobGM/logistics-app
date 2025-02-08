using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateSubscriptionPlanCommand : IRequest<Result>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
