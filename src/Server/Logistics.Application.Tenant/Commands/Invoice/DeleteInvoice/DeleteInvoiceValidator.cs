using FluentValidation;

namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteInvoiceValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeleteInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
