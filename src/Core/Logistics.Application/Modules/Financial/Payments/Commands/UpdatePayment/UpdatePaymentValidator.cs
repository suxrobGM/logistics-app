using FluentValidation;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class UpdatePaymentValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentValidator()
    {
        RuleFor(i => i.Id).NotEmpty();

        RuleFor(i => i.BillingAddress!)
            .SetValidator(new AddressValidator())
            .When(i => i.BillingAddress is not null);
    }
}
