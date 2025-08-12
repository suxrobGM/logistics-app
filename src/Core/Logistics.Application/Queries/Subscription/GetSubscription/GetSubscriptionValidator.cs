using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionValidator : AbstractValidator<GetSubscriptionQuery>
{
    public GetSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
