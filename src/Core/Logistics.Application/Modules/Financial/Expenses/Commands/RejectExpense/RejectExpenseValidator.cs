using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class RejectExpenseValidator : AbstractValidator<RejectExpenseCommand>
{
    public RejectExpenseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
