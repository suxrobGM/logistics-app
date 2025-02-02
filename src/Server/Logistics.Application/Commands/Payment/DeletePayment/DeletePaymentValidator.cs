using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeletePaymentValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
