using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class MoveContainerToTerminalValidator : AbstractValidator<MoveContainerToTerminalCommand>
{
    public MoveContainerToTerminalValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.TerminalId).NotEmpty();
    }
}
