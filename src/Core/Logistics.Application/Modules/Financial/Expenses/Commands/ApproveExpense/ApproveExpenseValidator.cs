using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class ApproveExpenseValidator : AbstractValidator<ApproveExpenseCommand>
{
    public ApproveExpenseValidator()
    {
        RuleFor(x => x.ApproverId).NotEmpty();
    }
}
