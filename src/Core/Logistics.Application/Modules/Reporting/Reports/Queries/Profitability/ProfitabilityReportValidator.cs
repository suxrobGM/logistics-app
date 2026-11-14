using FluentValidation;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class ProfitabilityReportValidator : AbstractValidator<ProfitabilityReportQuery>
{
    public ProfitabilityReportValidator()
    {
        RuleFor(q => q.FromDate).LessThan(q => q.ToDate);
        RuleFor(q => q.ToDate).LessThanOrEqualTo(q => q.FromDate.AddYears(1))
            .WithMessage("Pick a range of a year or less.");
    }
}
