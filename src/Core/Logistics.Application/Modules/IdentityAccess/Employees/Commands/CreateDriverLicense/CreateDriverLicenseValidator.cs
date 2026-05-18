using FluentValidation;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class CreateDriverLicenseValidator : AbstractValidator<CreateDriverLicenseCommand>
{
    public CreateDriverLicenseValidator()
    {
        RuleFor(i => i.EmployeeId).NotEmpty();
        RuleFor(i => i.LicenseNumber).NotEmpty().MaximumLength(64);

        RuleFor(i => i.IssuingCountry)
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Z]{2}$")
            .WithMessage("Issuing country must be an ISO 3166-1 alpha-2 code (e.g., 'US', 'DE').");

        RuleFor(i => i.IssuingRegion).MaximumLength(64);
        RuleFor(i => i.IssuedDate).NotEmpty();
        RuleFor(i => i.ExpiresAt)
            .NotEmpty()
            .GreaterThan(i => i.IssuedDate)
            .WithMessage("Expiry must be after issued date.");

        // License-class / country compatibility: US classes only with US country, EU classes only outside US.
        RuleFor(i => i.LicenseClass)
            .Must((cmd, cls) => IsClassCountryCompatible(cls, cmd.IssuingCountry))
            .WithMessage("License class is not valid for the issuing country.");
    }

    private static bool IsClassCountryCompatible(LicenseClass cls, string country)
    {
        var isUsClass = cls is LicenseClass.UsClassA or LicenseClass.UsClassB or LicenseClass.UsClassC;
        return isUsClass ? country == "US" : country != "US";
    }
}
