using FluentValidation;
using Logistics.Application.Modules.Financial.Payments.Commands;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class UpdateInvoiceValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
