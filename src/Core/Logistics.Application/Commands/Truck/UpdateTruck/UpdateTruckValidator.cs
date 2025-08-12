using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateTruckValidator : AbstractValidator<UpdateTruckCommand>
{
    public UpdateTruckValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.TruckType).IsInEnum();
        RuleFor(i => i.TruckStatus).IsInEnum();
    }
}
