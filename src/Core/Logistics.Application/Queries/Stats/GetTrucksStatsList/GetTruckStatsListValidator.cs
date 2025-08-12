using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTruckStatsListValidator : AbstractValidator<GetTrucksStatsListQuery>
{
    public GetTruckStatsListValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
