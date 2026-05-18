using FluentValidation;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

internal sealed class GetMonthlyGrossesValidator : AbstractValidator<GetMonthlyGrossesQuery>
{
    public GetMonthlyGrossesValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
    }
}
