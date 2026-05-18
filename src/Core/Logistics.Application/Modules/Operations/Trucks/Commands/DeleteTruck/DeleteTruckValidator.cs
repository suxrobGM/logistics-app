using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class DeleteTruckValidator : AbstractValidator<DeleteTruckCommand>
{
    public DeleteTruckValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
