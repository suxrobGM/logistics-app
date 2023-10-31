using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPayrollsValidator : AbstractValidator<GetPayrollsQuery>
{
    public GetPayrollsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
