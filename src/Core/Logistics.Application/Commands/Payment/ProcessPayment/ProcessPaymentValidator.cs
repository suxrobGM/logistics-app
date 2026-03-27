using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class ProcessPaymentValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentValidator()
    {
        RuleFor(i => i.PaymentId).NotEmpty();
    }
}
