using FluentValidation;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class MoveContainerToTerminalValidator : AbstractValidator<MoveContainerToTerminalCommand>
{
    public MoveContainerToTerminalValidator()
    {
        RuleFor(i => i.TerminalId).NotEmpty();
    }
}
