using FluentValidation;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class UpdateDriverPayProfileValidator : AbstractValidator<UpdateDriverPayProfileCommand>
{
    public UpdateDriverPayProfileValidator()
    {
        RuleForEach(i => i.RecurringDeductions).ChildRules(d =>
        {
            d.RuleFor(x => x.Type).IsInEnum();
            d.RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
            d.RuleFor(x => x.Amount).GreaterThan(0);
            d.RuleFor(x => x.EscrowTarget).GreaterThan(0);
            d.RuleFor(x => x.EscrowTarget).Null()
                .When(x => x.Type != PayDeductionType.Escrow)
                .WithMessage("Only an escrow deduction has a target balance.");
        });
    }
}
