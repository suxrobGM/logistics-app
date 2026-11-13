using FluentValidation;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class LoadsProfitabilityValidator : AbstractValidator<LoadsProfitabilityQuery>
{
    public LoadsProfitabilityValidator()
    {
        RuleFor(q => q.StartDate).LessThan(q => q.EndDate);
        RuleFor(q => q.EndDate).LessThanOrEqualTo(q => q.StartDate.AddYears(1))
            .WithMessage("Pick a range of a year or less.");
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).GreaterThanOrEqualTo(1);
    }
}
