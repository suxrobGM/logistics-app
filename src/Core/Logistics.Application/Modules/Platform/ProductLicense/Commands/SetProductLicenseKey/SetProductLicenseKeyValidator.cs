using FluentValidation;

namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

internal sealed class SetProductLicenseKeyValidator : AbstractValidator<SetProductLicenseKeyCommand>
{
    public SetProductLicenseKeyValidator()
    {
        // 4000 is the SystemSettings.Value column limit.
        RuleFor(i => i.Key).NotEmpty().MaximumLength(4000);
    }
}
