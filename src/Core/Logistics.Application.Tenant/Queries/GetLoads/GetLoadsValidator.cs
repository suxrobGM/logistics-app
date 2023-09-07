using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadsValidator : AbstractValidator<GetLoadsQuery>
{
    public GetLoadsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
