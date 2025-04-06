using FluentValidation;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionPlanValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
        
        // Validate the billing interval count, maximum of three years interval allowed (3 years, 36 months, or 156 weeks)
        RuleFor(i => i.IntervalCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(36)
            .When(i => i.Interval == BillingInterval.Month)
            .WithMessage("Interval count must be between 1 and 36 for monthly billing interval.");
        
        RuleFor(i => i.IntervalCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(3)
            .When(i => i.Interval == BillingInterval.Year)
            .WithMessage("Interval count must be between 1 and 3 for yearly billing interval.");
        
        RuleFor(i => i.IntervalCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(156)
            .When(i => i.Interval == BillingInterval.Week)
            .WithMessage("Interval count must be between 1 and 156 for weekly billing interval.");
        
        RuleFor(i => i.TrialPeriod)
            .IsInEnum()
            .WithMessage("Trial period must be a valid enum value.");
        
        RuleFor(i => i.Interval)
            .IsInEnum()
            .WithMessage("Billing interval must be a valid enum value.");
    }
}
