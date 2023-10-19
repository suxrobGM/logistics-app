using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPaymentsValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
