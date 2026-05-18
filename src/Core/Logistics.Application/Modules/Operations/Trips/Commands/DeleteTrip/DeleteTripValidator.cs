using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class DeleteTripValidator : AbstractValidator<DeleteTripCommand>
{
    public DeleteTripValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
