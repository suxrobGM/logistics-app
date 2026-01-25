using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RejectPayrollInvoiceValidator : AbstractValidator<RejectPayrollInvoiceCommand>
{
    public RejectPayrollInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Reason).NotEmpty().MaximumLength(1000);
    }
}
