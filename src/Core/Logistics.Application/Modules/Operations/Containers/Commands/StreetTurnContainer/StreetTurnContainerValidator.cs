using FluentValidation;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class StreetTurnContainerValidator : AbstractValidator<StreetTurnContainerCommand>
{
    public StreetTurnContainerValidator()
    {
        RuleFor(i => i.ExportBookingReference).NotEmpty().MaximumLength(100);
    }
}
