using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTrucksValidator : AbstractValidator<GetTrucksQuery>
{
    public GetTrucksValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);

        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
