using FluentValidation;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
