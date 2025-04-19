using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class SetDefaultPaymentMethodValidator : AbstractValidator<SetDefaultPaymentMethodCommand>
{
    public SetDefaultPaymentMethodValidator()
    {
        RuleFor(i => i.PaymentMethodId).NotEmpty();
    }
}
