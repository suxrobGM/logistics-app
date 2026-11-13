using FluentValidation;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class LanesProfitabilityValidator : AbstractValidator<LanesProfitabilityQuery>
{
    public LanesProfitabilityValidator()
    {
        RuleFor(q => q.StartDate).LessThan(q => q.EndDate);
        RuleFor(q => q.EndDate).LessThanOrEqualTo(q => q.StartDate.AddYears(1))
            .WithMessage("Pick a range of a year or less.");
    }
}
