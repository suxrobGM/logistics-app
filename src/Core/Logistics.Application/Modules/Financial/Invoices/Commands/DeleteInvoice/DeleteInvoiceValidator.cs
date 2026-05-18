using FluentValidation;
using Logistics.Application.Modules.Financial.Payments.Commands;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class DeleteInvoiceValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeleteInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
