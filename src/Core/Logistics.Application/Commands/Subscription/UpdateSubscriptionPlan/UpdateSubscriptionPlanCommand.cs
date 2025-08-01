using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateSubscriptionPlanCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public TrialPeriod? TrialPeriod { get; set; }
    public BillingInterval? Interval { get; set; }
    public int? IntervalCount { get; set; }
}
