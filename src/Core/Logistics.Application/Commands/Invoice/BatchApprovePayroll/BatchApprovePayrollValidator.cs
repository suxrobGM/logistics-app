using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class BatchApprovePayrollValidator : AbstractValidator<BatchApprovePayrollCommand>
{
    public BatchApprovePayrollValidator()
    {
        RuleFor(i => i.Ids).NotEmpty().WithMessage("At least one payroll ID is required");
        RuleFor(i => i.Notes).MaximumLength(1000);
    }
}
