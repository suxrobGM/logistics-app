using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class BatchCreatePayrollInvoicesValidator : AbstractValidator<BatchCreatePayrollInvoicesCommand>
{
    public BatchCreatePayrollInvoicesValidator()
    {
        RuleFor(x => x.EmployeeIds)
            .NotEmpty()
            .WithMessage("At least one employee must be selected.");

        RuleFor(x => x.PeriodStart)
            .LessThan(x => x.PeriodEnd)
            .WithMessage("Period start must be before period end.");

        RuleFor(x => x.EmployeeIds)
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("All employee IDs must be valid.");
    }
}
