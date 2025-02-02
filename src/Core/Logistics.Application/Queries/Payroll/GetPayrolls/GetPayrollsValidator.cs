using FluentValidation;

namespace Logistics.Application.Queries;

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
