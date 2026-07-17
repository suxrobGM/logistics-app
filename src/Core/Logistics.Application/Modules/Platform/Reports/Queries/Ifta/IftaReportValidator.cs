using FluentValidation;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

internal sealed class IftaReportValidator : AbstractValidator<IftaReportQuery>
{
    public IftaReportValidator()
    {
        RuleFor(i => i.Year).InclusiveBetween(2000, 2100);
        RuleFor(i => i.Quarter).InclusiveBetween(1, 4);
    }
}
