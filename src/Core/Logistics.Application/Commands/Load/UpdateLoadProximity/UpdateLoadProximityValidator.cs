using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadProximityValidator : AbstractValidator<UpdateLoadProximityCommand>
{
    public UpdateLoadProximityValidator()
    {
        RuleFor(i => i.LoadId).NotEmpty();
    }
}
