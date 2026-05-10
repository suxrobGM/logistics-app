using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateDriverLicenseValidator : AbstractValidator<UpdateDriverLicenseCommand>
{
    public UpdateDriverLicenseValidator()
    {
        RuleFor(i => i.LicenseId).NotEmpty();
        RuleFor(i => i.IssuingRegion).MaximumLength(64);
    }
}
