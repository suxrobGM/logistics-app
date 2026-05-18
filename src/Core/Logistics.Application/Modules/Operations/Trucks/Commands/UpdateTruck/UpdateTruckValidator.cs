using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class UpdateTruckValidator : AbstractValidator<UpdateTruckCommand>
{
    public UpdateTruckValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.TruckType).IsInEnum();
        RuleFor(i => i.TruckStatus).IsInEnum();
    }
}
