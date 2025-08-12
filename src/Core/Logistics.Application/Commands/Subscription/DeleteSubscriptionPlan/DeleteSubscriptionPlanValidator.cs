using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteSubscriptionPlanValidator : AbstractValidator<DeleteSubscriptionPlanCommand>
{
    public DeleteSubscriptionPlanValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
