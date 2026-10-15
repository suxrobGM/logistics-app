using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trailers.Queries;

internal sealed class GetTrailersValidator : AbstractValidator<GetTrailersQuery>
{
    public GetTrailersValidator()
    {
        RuleFor(i => i.Page).GreaterThanOrEqualTo(0);
        RuleFor(i => i.PageSize).GreaterThanOrEqualTo(1);
    }
}
