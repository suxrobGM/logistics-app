using FluentValidation;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class RequestCashAdvanceValidator : AbstractValidator<RequestCashAdvanceCommand>
{
    public RequestCashAdvanceValidator()
    {
        RuleFor(i => i.Amount).GreaterThan(0);
        RuleFor(i => i.Reason).MaximumLength(500);
    }
}
