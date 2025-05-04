using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RenewSubscriptionValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
