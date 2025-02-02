using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTenantsValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
