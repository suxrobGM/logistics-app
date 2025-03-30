using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionPaymentValidator : AbstractValidator<GetSubscriptionPaymentQuery>
{
    public GetSubscriptionPaymentValidator()
    {
        RuleFor(i => i.PaymentId).NotEmpty();
        RuleFor(i => i.SubscriptionId).NotEmpty();
    }
}
