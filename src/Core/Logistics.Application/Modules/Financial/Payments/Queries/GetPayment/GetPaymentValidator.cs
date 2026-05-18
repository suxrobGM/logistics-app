using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payments.Queries;

internal sealed class GetPaymentValidator : AbstractValidator<GetPaymentQuery>
{
    public GetPaymentValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
