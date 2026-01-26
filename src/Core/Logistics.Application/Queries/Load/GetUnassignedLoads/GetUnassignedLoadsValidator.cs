using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetUnassignedLoadsValidator : AbstractValidator<GetUnassignedLoadsQuery>
{
    public GetUnassignedLoadsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
