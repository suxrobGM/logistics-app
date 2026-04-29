using FluentValidation;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

internal sealed class UpdateContainerStatusValidator : AbstractValidator<UpdateContainerStatusCommand>
{
    public UpdateContainerStatusValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.TargetStatus).IsInEnum();
        When(i => i.TargetStatus is ContainerStatus.AtPort or ContainerStatus.Returned, () =>
        {
            RuleFor(i => i.TerminalId)
                .NotNull()
                .WithMessage("TerminalId is required for AtPort and Returned transitions.");
        });
    }
}
