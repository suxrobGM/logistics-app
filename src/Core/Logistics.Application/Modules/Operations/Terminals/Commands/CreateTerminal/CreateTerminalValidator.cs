using FluentValidation;
using Logistics.Application.Common.Validation;

namespace Logistics.Application.Modules.Operations.Terminals.Commands;

internal sealed class CreateTerminalValidator : AbstractValidator<CreateTerminalCommand>
{
    public CreateTerminalValidator()
    {
        RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
        RuleFor(i => i.Code).NotEmpty().Length(5).WithMessage("Terminal code must be 5 characters (UN/LOCODE).");
        RuleFor(i => i.Type).IsInEnum();
        RuleFor(i => i.Address).NotNull().SetValidator(new AddressValidator());
    }
}
