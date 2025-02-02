using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteSubscriptionValidator : AbstractValidator<DeleteSubscriptionCommand>
{
    public DeleteSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
