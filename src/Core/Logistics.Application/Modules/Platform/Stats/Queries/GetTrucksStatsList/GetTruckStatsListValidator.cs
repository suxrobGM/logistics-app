using FluentValidation;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

internal sealed class GetTruckStatsListValidator : AbstractValidator<GetTrucksStatsListQuery>
{
    public GetTruckStatsListValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(1);
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
