using FluentValidation;

namespace Logistics.Application.Queries;

public class GetExpensesValidator : AbstractValidator<GetExpensesQuery>
{
    public GetExpensesValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
