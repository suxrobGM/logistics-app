using FluentValidation;
using Logistics.Domain.Constraints;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadValidator()
    {
        RuleFor(i => i.AssignedDispatcherId).NotEmpty();
        RuleFor(i => i.AssignedDriverId).NotEmpty();
        RuleFor(i => i.SourceAddress).NotEmpty();
        RuleFor(i => i.DestinationAddress).NotEmpty();
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConsts.MinDeliveryCost)
            .LessThan(LoadConsts.MaxDeliveryCost);
    }
}
