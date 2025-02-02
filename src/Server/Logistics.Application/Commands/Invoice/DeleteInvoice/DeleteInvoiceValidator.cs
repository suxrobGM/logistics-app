using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteInvoiceValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeleteInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
