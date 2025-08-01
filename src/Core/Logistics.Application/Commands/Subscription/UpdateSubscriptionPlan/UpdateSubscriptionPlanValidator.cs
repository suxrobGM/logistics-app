using FluentValidation;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionPlanValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
        
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
    }
}
