using FluentValidation;

namespace Logistics.Application.Commands;

public class DeleteExpenseValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
