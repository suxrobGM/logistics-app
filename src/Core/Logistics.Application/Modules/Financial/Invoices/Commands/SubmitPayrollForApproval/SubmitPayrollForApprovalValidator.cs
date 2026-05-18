using FluentValidation;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class SubmitPayrollForApprovalValidator : AbstractValidator<SubmitPayrollForApprovalCommand>
{
    public SubmitPayrollForApprovalValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
