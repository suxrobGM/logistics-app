using System.Text.RegularExpressions;
using FluentValidation;
using Logistics.Shared.Models;

namespace Logistics.Application.Validators;

/// <summary>
/// One vehicle on a car hauler load. A unit must be identifiable by VIN or by make; a VIN, when
/// given, is the 17-character ISO 3779 form (no I, O or Q).
/// </summary>
internal sealed partial class LoadVehicleInputValidator : AbstractValidator<LoadVehicleInput>
{
    public LoadVehicleInputValidator()
    {
        RuleFor(v => v)
            .Must(v => !string.IsNullOrWhiteSpace(v.Vin) || !string.IsNullOrWhiteSpace(v.Make))
            .WithMessage("Each vehicle needs a VIN or a make.");

        RuleFor(v => v.Vin!)
            .Must(vin => VinPattern().IsMatch(vin.Trim().ToUpperInvariant()))
            .WithMessage("VIN must be 17 letters and digits, without I, O or Q.")
            .When(v => !string.IsNullOrWhiteSpace(v.Vin));

        RuleFor(v => v.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 2)
            .When(v => v.Year.HasValue);

        RuleFor(v => v.Rate).GreaterThanOrEqualTo(0).When(v => v.Rate.HasValue);
        RuleFor(v => v.Make).MaximumLength(100);
        RuleFor(v => v.Model).MaximumLength(100);
        RuleFor(v => v.Color).MaximumLength(50);
        RuleFor(v => v.LotNumber).MaximumLength(50);
        RuleFor(v => v.Notes).MaximumLength(500);
    }

    [GeneratedRegex("^[A-HJ-NPR-Z0-9]{17}$")]
    private static partial Regex VinPattern();
}
