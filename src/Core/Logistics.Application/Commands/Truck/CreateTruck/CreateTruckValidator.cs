using FluentValidation;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

internal sealed class CreateTruckValidator : AbstractValidator<CreateTruckCommand>
{
    public CreateTruckValidator()
    {
        RuleFor(i => i.TruckNumber).NotEmpty();
        RuleFor(i => i.TruckType).NotEmpty().IsInEnum();
        RuleFor(i => i.MainDriverId).NotEmpty();

        When(i => i.TruckType is TruckType.CarHauler, () =>
        {
            RuleFor(i => i.VehicleCapacity).NotEmpty().GreaterThan(0);
        });
    }
}
