using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class ApprovePayrollInvoiceValidator : AbstractValidator<ApprovePayrollInvoiceCommand>
{
    public ApprovePayrollInvoiceValidator()
    {
        RuleFor(i => i.Notes).MaximumLength(1000);
    }
}
