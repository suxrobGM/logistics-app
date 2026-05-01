using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DeleteTerminalValidator : AbstractValidator<DeleteTerminalCommand>
{
    public DeleteTerminalValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
