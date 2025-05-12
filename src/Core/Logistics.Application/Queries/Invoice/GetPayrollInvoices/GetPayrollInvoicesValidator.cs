using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetPayrollInvoicesValidator : AbstractValidator<GetPayrollInvoicesQuery>
{
    public GetPayrollInvoicesValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
