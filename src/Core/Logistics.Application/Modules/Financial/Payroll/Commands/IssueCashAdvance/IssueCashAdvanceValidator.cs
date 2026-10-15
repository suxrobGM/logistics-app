using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class IssueCashAdvanceValidator : AbstractValidator<IssueCashAdvanceCommand>
{
    public IssueCashAdvanceValidator()
    {
        RuleFor(i => i.Amount).GreaterThan(0);
        RuleFor(i => i.Reason).MaximumLength(500);
    }
}
