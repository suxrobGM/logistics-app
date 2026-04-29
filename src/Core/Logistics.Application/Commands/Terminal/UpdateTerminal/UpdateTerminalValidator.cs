using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateTerminalValidator : AbstractValidator<UpdateTerminalCommand>
{
    public UpdateTerminalValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        When(i => !string.IsNullOrEmpty(i.Code), () =>
        {
            RuleFor(i => i.Code).Length(5)
                .WithMessage("Terminal code must be 5 characters (UN/LOCODE).");
        });
        When(i => !string.IsNullOrEmpty(i.CountryCode), () =>
        {
            RuleFor(i => i.CountryCode).Length(2)
                .WithMessage("CountryCode must be a 2-letter ISO 3166-1 code.");
        });
    }
}
