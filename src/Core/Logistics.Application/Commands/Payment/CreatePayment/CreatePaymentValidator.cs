using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(i => i.Amount).GreaterThan(0M);
        RuleFor(i => i.PaymentMethodId).NotEmpty();
        RuleFor(i => i.BillingAddress).NotEmpty();
    }
}
