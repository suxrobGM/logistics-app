using FluentValidation;

namespace Logistics.Application.Modules.Reporting.Stats.Queries;

internal sealed class GetMonthlyGrossesValidator : AbstractValidator<GetMonthlyGrossesQuery>
{
    public GetMonthlyGrossesValidator()
    {
        RuleFor(i => i.FromDate).LessThan(i => i.ToDate);
    }
}
