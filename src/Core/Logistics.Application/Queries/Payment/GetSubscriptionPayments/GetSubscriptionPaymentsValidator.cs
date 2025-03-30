using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionPaymentsValidator : AbstractValidator<GetSubscriptionPaymentsQuery>
{
    public GetSubscriptionPaymentsValidator()
    {
        RuleFor(i => i.SubscriptionId).NotEmpty();
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
