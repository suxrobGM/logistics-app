using FluentValidation;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
