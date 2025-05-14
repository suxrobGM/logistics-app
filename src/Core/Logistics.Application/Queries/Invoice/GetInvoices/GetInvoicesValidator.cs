using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetInvoicesValidator : AbstractValidator<GetInvoicesQuery>
{
    public GetInvoicesValidator()
    {
        //RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
