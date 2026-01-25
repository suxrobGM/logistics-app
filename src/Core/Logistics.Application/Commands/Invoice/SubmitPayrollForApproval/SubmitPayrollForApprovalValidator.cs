using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class SubmitPayrollForApprovalValidator : AbstractValidator<SubmitPayrollForApprovalCommand>
{
    public SubmitPayrollForApprovalValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
