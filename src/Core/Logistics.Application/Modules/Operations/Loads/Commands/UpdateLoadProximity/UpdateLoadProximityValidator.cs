using FluentValidation;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class UpdateLoadProximityValidator : AbstractValidator<UpdateLoadProximityCommand>
{
    public UpdateLoadProximityValidator()
    {
        RuleFor(i => i.LoadId).NotEmpty();
    }
}
