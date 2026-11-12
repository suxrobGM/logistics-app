using FluentValidation;
using Logistics.Application.Modules.Operations.Loads.Constants;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class UpdateLoadValidator : AbstractValidator<UpdateLoadCommand>
{
    public UpdateLoadValidator()
    {
        Include(new LoadChargesValidator());
        Include(new LoadScheduleValidator());
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleForEach(i => i.Vehicles!)
            .SetValidator(new LoadVehicleInputValidator())
            .When(i => i.Vehicles is not null);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConstants.MinDeliveryCost)
            .LessThan(LoadConstants.MaxDeliveryCost);

        RuleFor(i => i.OriginAddress!)
            .SetValidator(new AddressValidator())
            .When(i => i.OriginAddress is not null);

        RuleFor(i => i.DestinationAddress!)
            .SetValidator(new AddressValidator())
            .When(i => i.DestinationAddress is not null);
    }
}
