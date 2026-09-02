using FluentValidation;

namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

internal sealed class RecordProductLicenseHeartbeatValidator : AbstractValidator<RecordProductLicenseHeartbeatCommand>
{
    public RecordProductLicenseHeartbeatValidator()
    {
        RuleFor(i => i.Report).NotNull();
        RuleFor(i => i.Report.InstanceId).NotEmpty();
        RuleFor(i => i.Report.Hostname).NotEmpty().MaximumLength(256);
        RuleFor(i => i.Report.Version).NotEmpty().MaximumLength(64);
        RuleFor(i => i.Report.KeyId).MaximumLength(64);
        RuleFor(i => i.Report.Licensee).MaximumLength(256);
        RuleFor(i => i.Report.TenantCount).GreaterThanOrEqualTo(0);
    }
}
