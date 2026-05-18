using FluentValidation;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(i => i.Amount).GreaterThan(0M);

        RuleFor(i => i.BillingAddress!)
            .SetValidator(new AddressValidator())
            .When(i => i.BillingAddress is not null);
    }
}
