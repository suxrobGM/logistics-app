using FluentValidation;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class RejectPayrollInvoiceValidator : AbstractValidator<RejectPayrollInvoiceCommand>
{
    public RejectPayrollInvoiceValidator()
    {
        RuleFor(i => i.Reason).NotEmpty().MaximumLength(1000);
    }
}
