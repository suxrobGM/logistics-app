using FluentValidation;
using Logistics.Application.Constants;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadValidator : AbstractValidator<UpdateLoadCommand>
{
    public UpdateLoadValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConstants.MinDeliveryCost)
            .LessThan(LoadConstants.MaxDeliveryCost);
    }
}
