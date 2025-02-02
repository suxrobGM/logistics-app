using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionPlansValidator : AbstractValidator<GetSubscriptionPlansQuery>
{
    public GetSubscriptionPlansValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
