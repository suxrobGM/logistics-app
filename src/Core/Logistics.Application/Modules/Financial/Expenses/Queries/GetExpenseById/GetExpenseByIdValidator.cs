using FluentValidation;

namespace Logistics.Application.Modules.Financial.Expenses.Queries;

public class GetExpenseByIdValidator : AbstractValidator<GetExpenseByIdQuery>
{
    public GetExpenseByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
