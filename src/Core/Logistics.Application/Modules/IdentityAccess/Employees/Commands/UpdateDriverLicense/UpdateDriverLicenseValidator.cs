using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class UpdateDriverLicenseValidator : AbstractValidator<UpdateDriverLicenseCommand>
{
    public UpdateDriverLicenseValidator()
    {
        RuleFor(i => i.LicenseId).NotEmpty();
        RuleFor(i => i.IssuingRegion).MaximumLength(64);
    }
}
