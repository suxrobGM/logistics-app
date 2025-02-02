using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(i => i.Amount).GreaterThan(0M);
    }
}
