using FluentValidation;

namespace Logistics.Application.Queries;

public class GetExpenseStatsValidator : AbstractValidator<GetExpenseStatsQuery>
{
    public GetExpenseStatsValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("From date must be before or equal to To date");
    }
}
