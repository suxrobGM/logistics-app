using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateSubscriptionPlanCommand : IRequest<Result>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public TrialPeriod TrialPeriod { get; set; }
    public BillingInterval Interval { get; set; }
    public int IntervalCount { get; set; }
}
