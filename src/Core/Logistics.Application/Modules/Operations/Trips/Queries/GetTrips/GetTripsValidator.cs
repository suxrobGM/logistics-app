using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

internal sealed class GetTripsValidator : AbstractValidator<GetTripsQuery>
{
    public GetTripsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);

        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
