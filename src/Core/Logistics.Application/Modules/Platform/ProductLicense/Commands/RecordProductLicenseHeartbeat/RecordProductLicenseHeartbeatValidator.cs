using FluentValidation;

namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

internal sealed class RecordProductLicenseHeartbeatValidator : AbstractValidator<RecordProductLicenseHeartbeatCommand>
{
    public RecordProductLicenseHeartbeatValidator()
    {
        RuleFor(i => i.InstanceId).NotEmpty();
        RuleFor(i => i.Hostname).NotEmpty().MaximumLength(256);
        RuleFor(i => i.Version).NotEmpty().MaximumLength(64);
        RuleFor(i => i.KeyId).MaximumLength(64);
        RuleFor(i => i.Licensee).MaximumLength(256);
        RuleFor(i => i.TenantCount).GreaterThanOrEqualTo(0);
    }
}
