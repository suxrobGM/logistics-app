using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class DeletePaymentValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
