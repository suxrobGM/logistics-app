using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CancelSubscriptionValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
