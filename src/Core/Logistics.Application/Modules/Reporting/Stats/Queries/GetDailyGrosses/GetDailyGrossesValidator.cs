using FluentValidation;

namespace Logistics.Application.Modules.Reporting.Stats.Queries;

internal sealed class GetDailyGrossesValidator : AbstractValidator<GetDailyGrossesQuery>
{
    public GetDailyGrossesValidator()
    {
        RuleFor(i => i.FromDate).NotNull();
        RuleFor(i => i.FromDate).LessThan(i => i.ToDate).When(i => i.ToDate.HasValue);
    }
}
