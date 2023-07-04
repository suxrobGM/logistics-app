using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal class GetTenantsValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(1);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}