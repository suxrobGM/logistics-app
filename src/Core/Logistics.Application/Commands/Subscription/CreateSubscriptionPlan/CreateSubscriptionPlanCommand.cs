using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class CreateSubscriptionPlanCommand : IAppRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public TrialPeriod TrialPeriod { get; set; }
    public BillingInterval Interval { get; set; }
    public int IntervalCount { get; set; }
}
