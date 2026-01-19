using FluentValidation;

namespace Logistics.Application.Queries;

public class GetExpenseByIdValidator : AbstractValidator<GetExpenseByIdQuery>
{
    public GetExpenseByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
