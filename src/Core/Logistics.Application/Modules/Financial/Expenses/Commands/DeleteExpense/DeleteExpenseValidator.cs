using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class DeleteExpenseValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
