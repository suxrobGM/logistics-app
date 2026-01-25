using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class ApprovePayrollInvoiceValidator : AbstractValidator<ApprovePayrollInvoiceCommand>
{
    public ApprovePayrollInvoiceValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Notes).MaximumLength(1000);
    }
}
