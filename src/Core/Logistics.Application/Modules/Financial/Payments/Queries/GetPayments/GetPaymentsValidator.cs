using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payments.Queries;

internal sealed class GetPaymentsValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsValidator()
    {
        RuleFor(i => i.FromDate).LessThan(i => i.ToDate).When(i => i.ToDate.HasValue);
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);

        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
