using FluentValidation;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

internal sealed class GetDailyGrossesValidator : AbstractValidator<GetDailyGrossesQuery>
{
    public GetDailyGrossesValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
    }
}
