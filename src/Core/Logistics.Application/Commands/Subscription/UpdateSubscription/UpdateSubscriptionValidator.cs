using FluentValidation;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
