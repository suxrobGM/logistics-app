using FluentValidation;
using Logistics.Application.Common.Validation;

namespace Logistics.Application.Modules.Operations.Terminals.Commands;

internal sealed class UpdateTerminalValidator : AbstractValidator<UpdateTerminalCommand>
{
    public UpdateTerminalValidator()
    {
        When(i => !string.IsNullOrEmpty(i.Code), () =>
        {
            RuleFor(i => i.Code).Length(5)
                .WithMessage("Terminal code must be 5 characters (UN/LOCODE).");
        });

        RuleFor(i => i.Address!)
            .SetValidator(new AddressValidator())
            .When(i => i.Address is not null);
    }
}
