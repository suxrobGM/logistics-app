using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTimeEntriesValidator : AbstractValidator<GetTimeEntriesQuery>
{
    public GetTimeEntriesValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
