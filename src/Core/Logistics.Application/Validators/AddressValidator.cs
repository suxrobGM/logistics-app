using FluentValidation;

using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Validators;

/// <summary>
/// Reusable validator for the <see cref="Address"/> value object.
/// State is required across all countries — the field doubles as
/// "State / Region / Province" with a country-aware label on the client.
/// </summary>
internal sealed class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(a => a.Line1)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(a => a.Line2)
            .MaximumLength(200);

        RuleFor(a => a.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(a => a.ZipCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(a => a.State)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("State / region / province is required.");

        RuleFor(a => a.Country)
            .NotEmpty()
            .Length(2)
            .WithMessage("Country must be a 2-letter ISO-3166-1 code.");
    }
}
