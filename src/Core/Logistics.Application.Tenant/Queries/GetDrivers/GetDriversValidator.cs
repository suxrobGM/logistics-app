using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriversValidator : AbstractValidator<GetDriversQuery>
{
    public GetDriversValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(1);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
