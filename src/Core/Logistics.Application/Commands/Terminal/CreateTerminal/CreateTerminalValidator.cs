using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateTerminalValidator : AbstractValidator<CreateTerminalCommand>
{
    public CreateTerminalValidator()
    {
        RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
        RuleFor(i => i.Code).NotEmpty().Length(5).WithMessage("Terminal code must be 5 characters (UN/LOCODE).");
        RuleFor(i => i.CountryCode).NotEmpty().Length(2).WithMessage("CountryCode must be a 2-letter ISO 3166-1 code.");
        RuleFor(i => i.Type).IsInEnum();
        RuleFor(i => i.Address).NotNull();
    }
}
