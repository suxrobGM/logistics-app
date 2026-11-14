using FluentValidation;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class ProfitabilityReportValidator : AbstractValidator<ProfitabilityReportQuery>
{
    public ProfitabilityReportValidator()
    {
        RuleFor(q => q.FromDate).NotNull().LessThan(q => q.ToDate);
        RuleFor(q => q.ToDate).NotNull()
            .LessThanOrEqualTo(q => q.FromDate!.Value.AddYears(1))
            .When(q => q.FromDate.HasValue)
            .WithMessage("Pick a range of a year or less.");
    }
}
